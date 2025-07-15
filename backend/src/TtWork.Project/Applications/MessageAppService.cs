using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Auditing;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using FreeIM;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nest;
using SqlSugar;
using TtWork.Abp;
using TtWork.Abp.Entity;
using TtWork.Project.Applications.GroupChatLevelSettings.Dto;
using TtWork.Project.Domains;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TtWork.Project.Applications;

public class MessageAppService(IRepository<Message, Guid> repository, ISqlSugarClient _sqlSugarClient)
    : AbpAppServiceBase
{
    [HttpGet]
    [AbpAuthorize]
    [DisableAuditing]
    public async Task<ListResultDto<ChatMessage>> GetChanHistory(string chan, long lastTime, int size = 20)
    {
        // 使用时间戳查询和排序
        var result = await repository.GetAll().AsNoTracking()
            .Where(x => x.Chan == chan && x.Time < lastTime)
            .OrderByDescending(x => x.Time)
            .Take(size)
            .ToListAsync();

        //群聊等级信息
        var groupChatLevel =
            await _sqlSugarClient.Queryable<GroupChatLevelSettingsEntity>().FirstAsync(f => f.Level == 0);
        //
        var userId = result.Select(s => s.From).ToList();
        //查询用户群聊等级
        var userGroupLevel = await _sqlSugarClient.Queryable<UserGroupLevelEntity>()
            .LeftJoin<GroupChatLevelSettingsEntity>((a, b) => a.GroupChatId == b.Id)
            .Where((a, b) => userId.Contains(a.UserId))
            .Select((a, b) => new
            {
                a.UserId,
                b.Name,
                b.Level,
                b.BorderColor,
                b.RightBorderColor
            })
            .ToListAsync();
        //
        // 按时间升序排列，确保消息顺序正确
        var orderedResult = result.OrderBy(x => x.Time).ToList();
        var list = new ListResultDto<ChatMessage>(ObjectMapper.Map<List<ChatMessage>>(orderedResult));

        foreach (var item in list.Items)
        {
            var info = userGroupLevel.Where(w => w.UserId == item.from).FirstOrDefault();
            if (info != null)
            {
                item.userChatLevel = new
                {
                    UserId = info.UserId,
                    Name = info.Name,
                    Level = info.Level,
                    BorderColor = info.BorderColor,
                    RightBorderColor = info.RightBorderColor
                };
            }
            else
            {
                item.userChatLevel = new
                {
                    UserId = groupChatLevel.Id,
                    Name = groupChatLevel.Name,
                    Level = groupChatLevel.Level,
                    BorderColor = groupChatLevel.BorderColor,
                    RightBorderColor = groupChatLevel.RightBorderColor
                };
            }

            // 处理拍卖消息payload的兼容性
            ProcessAuctionPayloadCompatibility(item);
        }

        return list;
    }

    [HttpGet]
    [AbpAuthorize]
    [DisableAuditing]
    public async Task<Guid> GetChanLastId(string chan)
    {
        var find = await repository.GetAll().AsNoTracking()
            .Where(x => x.Chan == chan)
            .OrderByDescending(x => x.Time)
            .Select(x => x.Id)
            .FirstOrDefaultAsync();
        return find;
    }

    /// <summary>
    /// 处理拍卖结束消息payload的兼容性问题
    /// 为AuctionEnd消息的关键字段添加大小写兼容性，解决小程序端显示问题
    /// </summary>
    /// <param name="message">聊天消息对象</param>
    private void ProcessAuctionPayloadCompatibility(ChatMessage message)
    {
        // 只处理拍卖结束消息类型
        if (message.type != ChatMessageType.AuctionEnd)
        {
            return;
        }

        try
        {
            // 如果payload是字符串，先解析为JObject
            JObject payloadObj;
            if (message.payload is string payloadStr)
            {
                payloadObj = JObject.Parse(payloadStr);
            }
            else if (message.payload is JObject jObj)
            {
                payloadObj = jObj;
            }
            else
            {
                // 如果不是JSON格式，直接返回
                return;
            }

            // 处理关键字段的兼容性
            var fieldsToProcess = new[]
            {
                ("Name", "name"),
                ("DealUserName", "dealUserName"),
                ("FinalPrice", "finalPrice")
            };

            foreach (var (upperField, lowerField) in fieldsToProcess)
            {
                // 如果存在大写字段，添加小写字段
                if (payloadObj.ContainsKey(upperField) && !payloadObj.ContainsKey(lowerField))
                {
                    payloadObj[lowerField] = payloadObj[upperField];
                }
                // 如果存在小写字段，添加大写字段
                else if (payloadObj.ContainsKey(lowerField) && !payloadObj.ContainsKey(upperField))
                {
                    payloadObj[upperField] = payloadObj[lowerField];
                }
            }

            // 更新消息的payload
            message.payload = payloadObj;
        }
        catch (Exception ex)
        {
            // 记录错误但不影响消息返回
            Logger.Error($"处理拍卖结束消息payload兼容性时出错: {ex.Message}");
        }
    }

    [HttpGet]
    [AbpAuthorize]
    [DisableAuditing]
    public async Task<ListResultDto<ChatMessage>> GetPrivateHistory(long id, long lastTime, int size = 20)
    {
        var myId = AbpSession.UserId!.Value;
        var result = await repository.GetAll().AsNoTracking()
            .Where(x =>
                ((x.From == id && x.To == myId) || (x.From == myId && x.To == id)) && x.Time < lastTime)
            .OrderByDescending(x => x.Time)
            .Take(size)
            .ToListAsync();

        //群聊等级信息
        var groupChatLevel =
            await _sqlSugarClient.Queryable<GroupChatLevelSettingsEntity>().FirstAsync(f => f.Level == 0);
        //
        var userId = result.Select(s => s.From).ToList();
        //查询用户群聊等级
        var userGroupLevel = await _sqlSugarClient.Queryable<UserGroupLevelEntity>()
            .LeftJoin<GroupChatLevelSettingsEntity>((a, b) => a.GroupChatId == b.Id)
            .Where((a, b) => userId.Contains(a.UserId))
            .Select((a, b) => new
            {
                a.UserId,
                b.Name,
                b.Level,
                b.BorderColor,
                b.RightBorderColor
            })
            .ToListAsync();
        //
        // 按时间升序排列，确保消息顺序正确
        var orderedResult = result.OrderBy(x => x.Time).ToList();
        var list = new ListResultDto<ChatMessage>(ObjectMapper.Map<List<ChatMessage>>(orderedResult));

        foreach (var item in list.Items)
        {
            var info = userGroupLevel.Where(w => w.UserId == item.from).FirstOrDefault();
            if (info != null)
            {
                item.userChatLevel = new
                {
                    UserId = info.UserId,
                    Name = info.Name,
                    Level = info.Level,
                    BorderColor = info.BorderColor,
                    RightBorderColor = info.RightBorderColor
                };
            }
            else
            {
                item.userChatLevel = new
                {
                    UserId = groupChatLevel.Id,
                    Name = groupChatLevel.Name,
                    Level = groupChatLevel.Level,
                    BorderColor = groupChatLevel.BorderColor,
                    RightBorderColor = groupChatLevel.RightBorderColor
                };
            }

            // 处理拍卖消息payload的兼容性
            ProcessAuctionPayloadCompatibility(item);
        }

        return list;
    }

    [HttpGet]
    [AbpAuthorize]
    [DisableAuditing]
    public async Task<Guid> GetPrivateLastId(long id)
    {
        var myId = AbpSession.UserId!.Value;
        var find = await repository.GetAll().AsNoTracking()
            .Where(x =>
                (x.From == id && x.To == myId) ||
                (x.From == myId && x.To == id))
            .OrderByDescending(x => x.Time)
            .Select(x => x.Id)
            .FirstOrDefaultAsync();
        return find;
    }
}