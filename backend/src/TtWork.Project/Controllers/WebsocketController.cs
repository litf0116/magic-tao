using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Controllers;
using Abp.Auditing;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Json;
using Abp.UI;
using Dapper;
using FreeIM;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SqlSugar;
using TtWork.Abp.Applications.Dtos;
using TtWork.Abp.Caches;
using TtWork.Abp.Dapper;
using TtWork.Abp.Entity;
using TtWork.Lib;
using TtWork.Project.Domains;
using TtWork.Project.Events;
using TtWork.Project.Events.Commands;
using static FreeSql.Internal.GlobalFilter;
using TtWork.Project.Applications.GroupChatLevelSettings.Dto;
using TtWork.Project.Services;

using Abp.Events.Bus;
using TtWork.Project.EventHandlers;

namespace TtWork.Project.Controllers
{
    public class SubscrChannelInput
    {
        public long WebsocketId { get; set; }
        public string Channel { get; set; }
    }

    public class SendChangeMsgInput
    {
        public long From { get; set; }
        public string Chan { get; set; }
        public ChatMessage Message { get; set; }
    }


    public record SendMsgInput
    {
        public Guid? Id { get; set; }
        public long From { get; set; }
        public long To { get; set; }
        public ChatMessage Message { get; set; }
        public bool IsReceipt { get; set; }
    }

    public record BanUserInput
    {
        public long UserId { get; set; }
        public long Minutes { get; set; }
        public string Chan { get; set; }
    }


    /// <summary>
    /// WebSocketController
    /// </summary>
    [Route("ws")]
    [DisableAuditing]
    public class WebSocketController(
        UserCache userCache,
        ISqlConnectionFactory sqlConnectionFactory,
        IRepository<Message, Guid> messageRepository,
        IHttpContextAccessor httpContextAccessor,
        IRepository<UserFriend> userFriendRepository,
        IRepository<BanedUser, long> banedUserRepository,
        IRepository<ChatListDelete> chatListDeleteRepository,
        IMediator mediator,
        ISqlSugarClient _sqlSugarClient,
        IMessageSequenceService messageSequenceService,
        IEventBus eventBus
    ) : AbpController
    {
        public string Ip
        {
            get
            {
                try
                {
                    return httpContextAccessor!.HttpContext!.Request.Headers["X-Real-IP"].FirstOrDefault() ??
                           httpContextAccessor!.HttpContext!.Request.HttpContext!.Connection!.RemoteIpAddress!
                               .ToString();
                }
                catch (Exception e)
                {
                    return "";
                }
            }
        }

        /// <summary>
        /// 获取websocket分区
        /// </summary>
        /// <param name="websocketId">本地标识，若无则不传，接口会返回新的，请保存本地localStoregy重复使用</param>
        /// <returns></returns>
        [HttpPost("pre-connect")]
        [AbpAuthorize]
        public object preConnect()
        {
            var websocketId = AbpSession.UserId!.Value;
            var server = ImHelper.PrevConnectServer(websocketId, this.Ip);
            return new
            {
                code = 0,
                server = server,
                websocketId = websocketId
            };
        }

        [HttpGet("offline")]
        public object Offline(long websocketId)
        {
            // ImHelper.ForceOffline(websocketId);
            return new
            {
                code = 0
            };
        }

        /// <summary>
        /// 群聊，获取群列表
        /// </summary>
        /// <returns></returns>
        [HttpPost("get-channels")]
        public object GetChannels()
        {
            return new
            {
                code = 0,
                channels = ImHelper.GetChanList().Select(a => new { a.chan, a.online }).Where(x => !x.chan.IsNullOrEmptyOrWhiteSpace())
            };
        }


        /// <summary>
        /// 群聊，绑定消息频道
        /// </summary>
        /// <param name="websocketId">本地标识，若无则不传，接口会返回，请保存本地重复使用</param>
        /// <param name="channel">消息频道</param>
        /// <returns></returns>
        [HttpPost("sub-channel")]
        [AbpAuthorize]
        public async Task<object> SubChannel([FromBody] SubscrChannelInput input)
        {
            var user = await userCache.GetAsync(AbpSession.UserId!.Value);
            if (!user.IsActive)
            {
                // 帐号禁用判断
                throw new UserFriendlyException(1, AppConsts.UserBanText);
            }

            if (input.Channel != "-1_auction")
                //组队场不修改不给发布招募信息
                if (Regex.IsMatch(user.Name, @"^玩家\d{5}"))
                {
                    throw new UserFriendlyException("请先修改昵称");
                }

            var name = input.Channel.Split('_')[1];
            var reg =
                @"^((?:[\u3400-\u4DB5\u4E00-\u9FEA\uFA0E\uFA0F\uFA11\uFA13\uFA14\uFA1F\uFA21\uFA23\uFA24\uFA27-\uFA29]|[\uD840-\uD868\uD86A-\uD86C\uD86F-\uD872\uD874-\uD879][\uDC00-\uDFFF]|\uD869[\uDC00-\uDED6\uDF00-\uDFFF]|\uD86D[\uDC00-\uDF34\uDF40-\uDFFF]|\uD86E[\uDC00-\uDC1D\uDC20-\uDFFF]|\uD873[\uDC00-\uDEA1\uDEB0-\uDFFF]|\uD87A[\uDC00-\uDFE0])|([0-9a-zA-Z])){4,12}$";
            if (!Regex.IsMatch(name, reg, RegexOptions.IgnoreCase))
            {
                throw new UserFriendlyException("群名称只能是4-12位中文或字母数字组合");
            }
            // }

            ImHelper.JoinChan(input.WebsocketId, input.Channel);
            // ImHelper.ClearChanClient(input.Channel);
            return new
            {
                code = 0
            };
        }

        [HttpGet("leave-channel")]
        public void LeaveChan(string chan)
        {
            ImHelper.LeaveChan(AbpSession.UserId!.Value, chan);
        }


        [HttpPost("del-channel")]
        [AbpAuthorize]
        public void DelChannel(string chan)
        {
            ImHelper.DeleteChan(chan);
            using var conn = sqlConnectionFactory.GetOpenConnection();
            conn.Execute("delete from t_message where chan=@chan", new { chan });
            // ImHelper.ClearChanClient(input.Channel)
        }

        /// <summary>
        /// 撤回消息
        /// </summary>
        [HttpPost("backout")]
        public async Task Backout([FromBody] ChatMessage input)
        {
            if (!input.id.HasValue) return;
            var isAdmin = await CheckIsChatAdmin(null);
            var message = await messageRepository.FirstOrDefaultAsync(input.id.Value);
            if (message?.Type is ChatMessageType.Text or ChatMessageType.Image or ChatMessageType.AuctionBid)
            {
                // 判断权限
                if (message.From == AbpSession.UserId!.Value || isAdmin.Item1)
                {
                    await messageRepository.DeleteAsync(input.id.Value);

                    if (message.Type == ChatMessageType.AuctionBid)
                    {
                        if (!isAdmin.Item1)
                        {
                            throw new UserFriendlyException("无权操作");
                        }

                        //修改拍卖状
                        var payload = message.Payload.FromJsonString<AuctionItemDto>();
                        if (payload is { Id: > 0 })
                        {
                            await mediator.Publish(new RollBackAuctionEvent(payload));
                        }
                    }

                    await CurrentUnitOfWork.SaveChangesAsync();

                    if (!message.Chan.IsNullOrWhiteSpace())
                        ImHelper.SendChanMessage(0, message.Chan, new ChatMessage
                        {
                            id = message.Id,
                            type = ChatMessageType.Backout,
                            msg = isAdmin.Item1 ? "管理员撤回了一条消息" : $"{message.FromName} 撤回了一条消息",
                            chan = message.Chan
                        });
                    else
                        ImHelper.SendMessage(0, [message.From, message.To!.Value], new ChatMessage
                        {
                            id = message.Id,
                            type = ChatMessageType.Backout,
                            msg = isAdmin.Item1 ? "管理员撤回了一条消息" : $"{message.FromName} 撤回了一条消息",
                        });
                }
            }
        }

        /// <summary>
        /// 用户禁言
        /// </summary>
        /// <param name="input"></param>
        [HttpPost("ban-user")]
        public async Task BanUser([FromBody] BanUserInput input)
        {
            var isChatAdmin = await CheckIsChatAdmin(null);
            if (!isChatAdmin.Item1) throw new UserFriendlyException("无权操作");

            await banedUserRepository.InsertAsync(new BanedUser(input.UserId, input.Minutes, input.Chan));
            await CurrentUnitOfWork.SaveChangesAsync();

            var user = await userCache.GetAsync(input.UserId);
            var info = user != null ? user.Name : input.UserId.ToString();
            ImHelper.SendChanMessage(0, input.Chan, new ChatMessage
            {
                type = ChatMessageType.BanUser,
                msg = $"{info}已被禁言{input.Minutes}分钟",
                chan = input.Chan
            });
        }
        /// <summary>
        /// 群聊，发送频道消息，绑定频道的所有人将收到消息
        /// </summary>
        /// <returns></returns>
        [HttpPost("SendChannelMsg")]
        // [HttpPost]
        public async Task SendChannelMsg([FromBody] SendChangeMsgInput input)
        {
            var cacheUser = await userCache.GetAsync(AbpSession.UserId!.Value);
            if (!cacheUser.IsActive)
            {
                // 帐号禁用判断
                throw new UserFriendlyException(1, AppConsts.UserBanText);
            }

            input.From = cacheUser.Id;
            
            //移除'玩家xxxxx加入群聊'的提示
            //只显示已经修改过名字和头像的玩家的提示   
            if (input.Message is { type: ChatMessageType.Welcome })
            {
                // if (Regex.IsMatch(input.Message.fromName, @"^玩家\d{5}"))
                if (input.Chan is "-1_auction")
                    return;
            }

            input.Message = await CheckMsgText(input.Message);
            input.Message.id = Guid.NewGuid();
            input.Message.chan = input.Chan.ToString();
            
            // 使用后端缓存的用户头像，不依赖前端参数
            input.Message.avatar = NormalizeAvatarUrl(cacheUser.HeadImgUrl);
            input.Message.fromName = cacheUser.Name;

            var isChatAdmin = await CheckIsChatAdmin(cacheUser);

            input.Message.fromAdmin = isChatAdmin.Item1;
            input.Message.fromTag = isChatAdmin.Item2;
            input.Message.tagClass = isChatAdmin.Item3;

            if (!isChatAdmin.Item1)
            {
                // 非管理判断是否被禁言
                var banedUser = await banedUserRepository.FirstOrDefaultAsync(a =>
                    a.UserId == AbpSession.UserId!.Value && (a.Chan == null || a.Chan == input.Chan) &&
                    a.EndTime > DateTime.Now);
                if (banedUser != null)
                {
                    throw new UserFriendlyException($"您已被禁言,结束时间 {banedUser.EndTime:yyyy-MM-dd HH:mm:ss}");
                }
            }

            // 生成序列号
            var sequenceNumber = await messageSequenceService.GetNextSequenceNumberForChannelAsync(input.Chan);

            #region 设置用户群聊等级信息
            //群聊等级信息
            var groupChatLevel = await _sqlSugarClient.Queryable<GroupChatLevelSettingsEntity>().FirstAsync(f => f.Level == 0);
            //查询用户群聊等级
            var userGroupLevel = await _sqlSugarClient.Queryable<UserGroupLevelEntity>()
                    .LeftJoin<GroupChatLevelSettingsEntity>((a, b) => a.GroupChatId == b.Id)
                    .Where((a, b) => a.UserId == input.From)
                    .Select((a, b) => new
                    {
                        a.UserId,
                        b.Name,
                        b.Level,
                        b.BorderColor,
                        b.RightBorderColor
                    })
                    .FirstAsync();
            //设置用户群聊等级信息
            if (userGroupLevel != null)
            {
                input.Message.userChatLevel = new
                {
                    userId = userGroupLevel.UserId,
                    name = userGroupLevel.Name,
                    level = userGroupLevel.Level,
                    borderColor = userGroupLevel.BorderColor,
                    rightBorderColor = userGroupLevel.RightBorderColor
                };
            }
            else
            {
                input.Message.userChatLevel = new
                {
                    userId = groupChatLevel.Id,
                    name = groupChatLevel.Name,
                    level = groupChatLevel.Level,
                    borderColor = groupChatLevel.BorderColor,
                    rightBorderColor = groupChatLevel.RightBorderColor
                };
            }
            #endregion

            if (input.Message.type != ChatMessageType.Welcome)
            {
                var entity = new Message(input.Message, sequenceNumber)
                {
                    Ip = Ip,
                    FromAdmin = isChatAdmin.Item1,
                    FromTag = isChatAdmin.Item2,
                    TagClass = isChatAdmin.Item3
                };
                await messageRepository.InsertAsync(entity);
                await CurrentUnitOfWork.SaveChangesAsync();

                // 使用服务端生成的时间戳更新消息
                input.Message.time = entity.Time;
                input.Message.sequenceNumber = entity.SequenceNumber;

                // 触发聊天消息发送事件，异步更新ChatChannel表
                await eventBus.TriggerAsync(new ChatMessageSentEvent(entity.Id));
            }

            //判断input.form在不在redis的chan里
            ImHelper.SendChanMessage(input.From, input.Chan, input.Message);
        }


        /// <summary>
        /// 单聊
        /// </summary>
        /// <param name="senderWebsocketId">发送者</param>
        /// <param name="receiveWebsocketId">接收者</param>
        /// <param name="message">发送内容</param>
        /// <param name="isReceipt">是否需要回执</param>
        /// <returns></returns>
        [HttpPost("send-msg")]
        public async Task<object> SendMsg([FromBody] SendMsgInput input)
        {
            var cacheUser = await userCache.GetAsync(AbpSession.UserId!.Value);
            if (!cacheUser.IsActive)
            {
                // 帐号禁用判断
                throw new UserFriendlyException(1, AppConsts.UserBanText);
            }

            input.Message = await CheckMsgText(input.Message);
            input.Message.id = Guid.NewGuid();
            input.Message.to ??= input.To;
            
            input.Message.avatar = NormalizeAvatarUrl(cacheUser.HeadImgUrl);
            input.Message.fromName = cacheUser.Name;

            var isChatAdmin = await CheckIsChatAdmin(cacheUser);

            input.Message.fromAdmin = isChatAdmin.Item1;
            input.Message.fromTag = isChatAdmin.Item2;
            input.Message.tagClass = isChatAdmin.Item3;

            // 生成序列号
            var sequenceNumber = await messageSequenceService.GetNextSequenceNumberForPrivateAsync(input.From, input.To);

            //TODO 判断是否是好友,管理员可以随便发送
            //var loginUser = 发送者;
            //var recieveUser = User.Get(receiveWebsocketId);
            //if (loginUser.好友 != recieveUser) throw new Exception("不是好友");
            #region 设置用户群聊等级信息
            //群聊等级信息
            var groupChatLevel = await _sqlSugarClient.Queryable<GroupChatLevelSettingsEntity>().FirstAsync(f => f.Level == 0);
            //查询用户群聊等级
            var userGroupLevel = await _sqlSugarClient.Queryable<UserGroupLevelEntity>()
                    .LeftJoin<GroupChatLevelSettingsEntity>((a, b) => a.GroupChatId == b.Id)
                    .Where((a, b) => a.UserId == input.From)
                    .Select((a, b) => new
                    {
                        a.UserId,
                        b.Name,
                        b.Level,
                        b.BorderColor,
                        b.RightBorderColor
                    })
                    .FirstAsync();
            //设置用户群聊等级信息
            if (userGroupLevel != null)
            {
                input.Message.userChatLevel = new
                {
                    userId = userGroupLevel.UserId,
                    name = userGroupLevel.Name,
                    level = userGroupLevel.Level,
                    borderColor = userGroupLevel.BorderColor,
                    rightBorderColor = userGroupLevel.RightBorderColor
                };
            }
            else
            {
                input.Message.userChatLevel = new
                {
                    userId = groupChatLevel.Id,
                    name = groupChatLevel.Name,
                    level = groupChatLevel.Level,
                    borderColor = groupChatLevel.BorderColor,
                    rightBorderColor = groupChatLevel.RightBorderColor
                };
            }
            #endregion

            //loginUser.保存记录(message);
            //recieveUser.保存记录(message);

            var entity = new Message(input.Message, sequenceNumber)
            {
                Ip = Ip,
                FromAdmin = isChatAdmin.Item1,
                FromTag = isChatAdmin.Item2,
                TagClass = isChatAdmin.Item3
            };

            await messageRepository.InsertAsync(entity);
            await CurrentUnitOfWork.SaveChangesAsync();

            // 使用服务端生成的时间戳更新消息
            input.Message.time = entity.Time;
            input.Message.sequenceNumber = entity.SequenceNumber;

            // 触发聊天消息发送事件，异步更新ChatChannel表
            await eventBus.TriggerAsync(new ChatMessageSentEvent(entity.Id));

            ImHelper.SendMessage(input.From, [input.To], input.Message,
                input.IsReceipt);

            // 删除相关的聊天列表删除记录 - 采用最安全的方式：先查询再逐个删除
            var deleteRecords = await chatListDeleteRepository.GetAll()
                .Where(x => (x.UserId == AbpSession.UserId.Value && x.ToUserId == entity.To) || 
                           (x.UserId == entity.To && x.ToUserId == AbpSession.UserId.Value))
                .ToListAsync();
            
            if (deleteRecords.Any())
            {
                foreach (var record in deleteRecords)
                {
                    await chatListDeleteRepository.DeleteAsync(record);
                }
            }

            return new
            {
                code = 0,
                data = input with { Id = entity.Id, Message = input.Message }
            };
        }

        private async Task<(bool, string, string)> CheckIsChatAdmin(UserDto currentUser)
        {
            try
            {
                currentUser ??= await userCache.GetAsync(AbpSession.UserId!.Value);
                if (currentUser is { RoleNames.Length: > 0 })
                {
                    if (currentUser.RoleNames.Contains("AuctionManager"))
                        return (true, "拍卖师", "tag_AuctionManager");
                    if (currentUser.RoleNames.Contains("Manager"))
                        return (true, "管理员", "tag_Manager");
                    if (currentUser.RoleNames.Contains("AuctionUser"))
                        return (false, "竞拍用户", "tag_AudtionUser");
                    if (currentUser.RoleNames.Contains("Admin"))
                        return (true, "系统管理员", "tag_Admin");
                }
            }
            catch (Exception e)
            {
                Logger.Error("获取用户缓存信息失败", e);
            }

            return (false, "", "");
        }


        private async Task<ChatMessage> CheckMsgText(ChatMessage message)
        {
            //从Redis缓存中取出敏感词
            var sw = await mediator.Send(new QueryCacheWords());

            var result = IndexOfFirstArray(message.msg, sw);
            if (result is not null)
            {
                throw new UserFriendlyException($"含有禁用词:{result}");
            }

            if (message.msg != null && message.msg.Length > 400) throw new UserFriendlyException("消息过长");

            message.msg = HttpUtility.HtmlEncode(message.msg);
            return message;
        }

        /// <summary>
        /// 检查敏感词
        /// </summary>
        /// <param name="text">原文</param>
        /// <param name="needles">敏感词数组</param>
        /// <returns>首个匹配到的敏感词</returns>
        private string IndexOfFirstArray(string text, string[] needles)
        {
            ReadOnlySpan<char> haystatck = text;
            for (var i = 0; i < haystatck.Length; i++)
            {
                foreach (var needle in needles)
                {
                    if (!string.IsNullOrEmpty(needle))
                        if (haystatck[i..].StartsWith(needle, StringComparison.OrdinalIgnoreCase))
                        {
                            return needle;
                        }
                }
            }

            return null;
        }

        private static string NormalizeAvatarUrl(string url)
        {
            if (string.IsNullOrEmpty(url)) return url;
            
            return url
                .Replace("cdn.molitao.top", "image.molitao.top")
                .Replace("http://image.molitao.top", "https://image.molitao.top");
        }
    }
}

