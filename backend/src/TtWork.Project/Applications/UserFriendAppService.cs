using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Auditing;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.UI;
using FreeIM;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TtWork.Abp;
using TtWork.Abp.Applications.Dtos;
using TtWork.Abp.Caches;
using TtWork.Project.Domains;
using TtWork.Project.Services.Messaging;

namespace TtWork.Project.Applications;

[AbpAuthorize]
public class UserFriendAppService(
    IRepository<UserFriend> repository,
    UserCache userCache,
    IMessageSendingService messageSendingService
) : AbpAppServiceBase {
    [HttpGet]
    public async Task AddFriend(long id) {
        if (id != AbpSession.UserId) {
            // 查询：对方是否已经发给我好友请求（UserId=对方, FriendId=我）
            var entity = await repository.FirstOrDefaultAsync(x =>
                x.UserId == id &&
                x.FriendId == AbpSession.UserId!.Value);
            if (entity != null) {
                if (entity.Status)
                    throw new UserFriendlyException("对方已是你的好友");
                throw new UserFriendlyException("请不要重复发送好友请求");
            }

            // 插入：UserId=对方（接收方），FriendId=我（申请方）
            await repository.InsertAsync(new UserFriend {
                UserId = id,
                FriendId = AbpSession.UserId!.Value,
                Status = false
            });

            await CurrentUnitOfWork.SaveChangesAsync();
        }
    }

    /// <summary>
    /// 获取发给我的好友申请列表
    /// </summary>
    /// <param name="id">当前用户ID（从会话获取，可忽略传入值）</param>
    /// <param name="status">false=待处理申请，true=已同意好友</param>
    [HttpGet]
    public async Task<ListResultDto<UserDtoBase>> GetUserFriends(long id, bool status) {
        var userId = AbpSession.UserId!.Value;
        // 查询：FriendId=我（我是接收方），Status=状态
        var list = await repository.GetAll().AsNoTracking()
            .Where(x => x.FriendId == userId && x.Status == status)
            .ToListAsync();
        List<UserDtoBase> result = [];
        foreach (var u in list) {
            // 获取申请人信息（UserId=申请人）
            var cache = await userCache.GetAsync(u.UserId);
            if (cache != null)
                result.Add(ObjectMapper.Map<UserDtoBase>(cache));
        }

        return new ListResultDto<UserDtoBase>(result);
    }

    /// <summary>
    /// 获取发给我的好友申请数量（红点数量）
    /// </summary>
    [HttpGet]
    [DisableAuditing]
    public async Task<int> GetUserFriendCount()
    {
        var userId = AbpSession.UserId!.Value;
        // 查询：FriendId=我（我是接收方），Status=false（待处理）
        var count = await repository.GetAll().AsNoTracking()
            .CountAsync(x => x.FriendId == userId && x.Status == false);
        return count;
    }

    /// <summary>
    /// 同意或拒绝好友申请
    /// </summary>
    /// <param name="id">申请人ID（发起好友请求的人）</param>
    /// <param name="status">true=同意，false=拒绝</param>
    [HttpGet]
    public async Task Agree(long id, bool status) {
        var userId = AbpSession.UserId!.Value;
        // 查询：FriendId=我（我是接收方），UserId=对方（申请方）
        var entity = await repository.FirstOrDefaultAsync(x =>
            x.FriendId == userId &&
            x.UserId == id);
        if (entity == null)
            throw new UserFriendlyException("记录不存在");

        if (entity.Status)
            throw new UserFriendlyException("该好友请求已处理");

        // 获取信息：申请人是id，我（处理方）是userId
        var senderUser = await userCache.GetAsync(userId);  // 处理人
        var friendUser = await userCache.GetAsync(id);     // 申请人
        var senderName = senderUser?.Name ?? "用户";

        if (status) {
            entity.Status = true;

            // 创建反向好友关系（如果不存在）
            var t = await repository.FirstOrDefaultAsync(x => x.UserId == id && x.FriendId == userId);
            if (t != null) {
                t.Status = true;
            }
            else {
                await repository.InsertAsync(new UserFriend {
                    UserId = id,
                    FriendId = userId,
                    Status = true
                });
            }

            await CurrentUnitOfWork.SaveChangesAsync();

            // 通知申请人
            if (friendUser != null) {
                await messageSendingService.SendSystemPrivateMessageAsync(id, new ChatMessage {
                    type = ChatMessageType.Text,
                    msg = $"\"{senderName}\" 已同意你的好友请求",
                    to = id
                });
            }
        }
        else {
            // 拒绝：删除这条申请记录
            if (friendUser != null) {
                await messageSendingService.SendSystemPrivateMessageAsync(id, new ChatMessage {
                    type = ChatMessageType.Text,
                    msg = $"\"{senderName}\" 拒绝了你的好友请求",
                    to = id
                });
            }

            await repository.DeleteAsync(entity);
            await CurrentUnitOfWork.SaveChangesAsync();
        }
    }
}