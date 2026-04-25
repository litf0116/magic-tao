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
            var entity = await repository.FirstOrDefaultAsync(x =>
                x.UserId == id &&
                x.FriendId == AbpSession.UserId!.Value);
            if (entity != null) {
                if (entity.Status)
                    throw new UserFriendlyException("对方已是你的好友");
                throw new UserFriendlyException("请不要重复发送好友请求");
            }

            await repository.InsertAsync(new UserFriend {
                UserId = id,
                FriendId = AbpSession.UserId!.Value,
                Status = false
            });

            await CurrentUnitOfWork.SaveChangesAsync();
        }
    }

    [HttpGet]
    public async Task<ListResultDto<UserDtoBase>> GetUserFriends(long id, bool status) {
        var list = await repository.GetAll().AsNoTracking().Where(x => x.UserId == id && x.Status == status)
            .ToListAsync();
        List<UserDtoBase> result = [];
        foreach (var u in list) {
            var cache = await userCache.GetAsync(u.FriendId);
            if (cache != null) {
                result.Add(ObjectMapper.Map<UserDtoBase>(cache));
            }
        }

        return new ListResultDto<UserDtoBase>(result);
    }

    [HttpGet]
    [DisableAuditing]
    public async Task<object> GetUserFriendCount()
    {
        var userId = AbpSession.UserId!.Value;
        var count = await repository.GetAll().AsNoTracking().CountAsync(x => x.UserId == userId && x.Status == false);
        return new { count };
    }

    [HttpGet]
    public async Task Agree(long id, bool status) {
        var userId = AbpSession.UserId!.Value;
        var entity = await repository.FirstOrDefaultAsync(x => x.UserId == userId &&
                                                               x.FriendId == id &&
                                                               x.Status == false);
        if (entity == null)
            throw new UserFriendlyException("记录不存在");

        var senderName = await GetUserNameAsync(userId);
        var friendUserId = id;

        if (status) {
            entity.Status = true;

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

            await messageSendingService.SendSystemPrivateMessageAsync(friendUserId, new ChatMessage {
                type = ChatMessageType.Text,
                msg = $"\"{senderName}\" 已同意你的好友请求",
                to = friendUserId
            });
        }
        else {
            await messageSendingService.SendSystemPrivateMessageAsync(friendUserId, new ChatMessage {
                type = ChatMessageType.Text,
                msg = $"\"{senderName}\" 拒绝了你的好友请求",
                to = friendUserId
            });

            await repository.DeleteAsync(entity);
            await CurrentUnitOfWork.SaveChangesAsync();
        }
    }

    private async Task<string> GetUserNameAsync(long userId) {
        var cache = await userCache.GetAsync(userId);
        return cache?.Name ?? cache?.UserName ?? "用户";
    }
}