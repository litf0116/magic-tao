using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Controllers;
using Abp.Auditing;
using Abp.Domain.Repositories;
using FreeIM;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TtWork.Abp.Caches;
using TtWork.Project.Domains;
using TtWork.Project.Services;
using TtWork.Project.Services.Messaging;
using TtWork.Project.Services.Messaging.Models;

namespace TtWork.Project.Controllers
{
    public class TestSendChannelMsgInput
    {
        public long From { get; set; }
        public string Chan { get; set; }
        public ChatMessage Message { get; set; }
    }

    /// <summary>
    /// 消息发送测试控制器
    /// </summary>
    [Route("api/test/message")]
    [DisableAuditing]
    public class MessageTestController : AbpController
    {
        private readonly IMessageSendingService _messageSendingService;
        private readonly IRepository<ChatChannel, long> _chatChannelRepository;
        private readonly ChatChannelService _chatChannelService;
        private readonly ChatUserCache _chatUserCache;

        public MessageTestController(
            IMessageSendingService messageSendingService,
            IRepository<ChatChannel, long> chatChannelRepository,
            ChatChannelService chatChannelService,
            ChatUserCache chatUserCache)
        {
            _messageSendingService = messageSendingService;
            _chatChannelRepository = chatChannelRepository;
            _chatChannelService = chatChannelService;
            _chatUserCache = chatUserCache;
        }

        /// <summary>
        /// 测试查询 ChatChannel 表
        /// </summary>
        [HttpGet("channels")]
        public async Task<object> TestChannels()
        {
            var channels = await _chatChannelRepository.GetAll()
                .IgnoreQueryFilters()
                .Where(c => c.IsActive && c.LastMessageId != null)
                .ToListAsync();

            return new
            {
                success = true,
                count = channels.Count,
                channels = channels.Select(c => new
                {
                    c.Id,
                    c.ChannelId,
                    c.ChannelType,
                    c.ChannelName,
                    c.User1Id,
                    c.User2Id,
                    c.LastMessageId,
                    c.LastMessageContent,
                    c.IsActive
                })
            };
        }

        /// <summary>
        /// 测试获取聊天列表 (模拟 GetChatList)
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>聊天列表</returns>
        [HttpGet("chat-list")]
        public async Task<object> TestGetChatList([FromQuery] long userId)
        {
            if (userId <= 0)
            {
                return new
                {
                    success = false,
                    message = "请提供有效的用户ID"
                };
            }

            var channels = await _chatChannelService.GetVisibleChannelsForUserAsync(userId);
            if (channels.Count == 0)
            {
                return new
                {
                    success = true,
                    count = 0,
                    chatList = new object[0],
                    message = "没有可见的聊天频道"
                };
            }

            var privateUserIds = channels
                .Where(c => c.ChannelType == ChatChannelType.Private)
                .Select(c => c.GetOtherUserId(userId) ?? 0)
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            var userInfos = await _chatUserCache.GetBatchUserBasicAsync(privateUserIds);

            var chatList = channels.Select(c =>
            {
                if (c.ChannelType == ChatChannelType.System)
                {
                    return new
                    {
                        id = c.ChannelId switch
                        {
                            "-1_auction" => -1,
                            "0_lobby" => 0,
                            _ => c.ChannelId.GetHashCode()
                        },
                        name = c.ChannelName ?? c.ChannelId,
                        lastMsg = c.LastMessageContent ?? "",
                        time = c.LastMessageTime,
                        type = 0,
                        unread = 0,
                        avatar = "",
                        order = c.SortOrder
                    };
                }

                if (c.ChannelType == ChatChannelType.Private)
                {
                    var otherId = c.GetOtherUserId(userId);
                    if (otherId.HasValue && userInfos.TryGetValue(otherId.Value, out var info))
                    {
                        return new
                        {
                            id = otherId.Value,
                            name = info.Name,
                            avatar = info.HeadImgUrl ?? "",
                            lastMsg = c.LastMessageContent ?? "",
                            time = c.LastMessageTime,
                            type = 1,
                            unread = 0,
                            order = c.SortOrder
                        };
                    }
                }

                return (object)null!;
            })
                .Where(x => x != null)
                .ToList();

            return new
            {
                success = true,
                count = chatList.Count,
                chatList = chatList,
                message = $"用户 {userId} 的聊天列表"
            };
        }

        /// <summary>
        /// 同步所有聊天频道
        /// 根据 Message 表数据创建或更新 ChatChannel 表记录
        /// </summary>
        /// <param name="maxBatchCount">最大处理用户数（分批处理避免内存溢出）</param>
        /// <returns>同步结果统计</returns>
        [HttpPost("channels/sync-all")]
        public async Task<object> SyncAllChannels([FromQuery] int maxBatchCount = 1000)
        {
            var result = await _chatChannelService.SyncChannelsFromMessageAsync(maxBatchCount);
            return new
            {
                success = result.IsSuccess,
                message = result.IsSuccess ? "同步成功" : "同步失败",
                data = new
                {
                    result.StartTime,
                    result.EndTime,
                    result.Duration,
                    result.TotalActiveUsers,
                    result.SystemChannelsCreated,
                    result.SystemChannelsUpdated,
                    result.PrivateChannelsCreated,
                    result.PrivateChannelsUpdated,
                    result.IsSuccess,
                    result.ErrorMessage
                }
            };
        }

        /// <summary>
        /// 同步单个用户的私聊频道
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>同步结果</returns>
        [HttpPost("channels/sync-user")]
        public async Task<object> SyncUserChannels([FromQuery] long userId)
        {
            if (userId <= 0)
            {
                return new
                {
                    success = false,
                    message = "请提供有效的用户ID"
                };
            }

            var result = await _chatChannelService.SyncUserChannelsAsync(userId);
            return new
            {
                success = result.IsSuccess,
                message = result.IsSuccess ? "同步成功" : "同步失败",
                data = new
                {
                    result.UserId,
                    result.TotalChannels,
                    result.CreatedChannels,
                    result.UpdatedChannels,
                    result.StartTime,
                    result.EndTime,
                    result.Duration,
                    result.IsSuccess,
                    result.ErrorMessage
                }
            };
        }

        /// <summary>
        /// 测试发送群组消息
        /// </summary>
        /// <param name="input">发送消息的输入参数</param>
        /// <returns>发送结果</returns>
        [HttpPost("send-channel")]
        public async Task<object> SendChannelMessageTest([FromBody] TestSendChannelMsgInput input)
        {
            var options = new MessageSendOptions
            {
                SkipPermissionCheck = true,
                SkipSensitiveWordCheck = true,
                PersistToDatabase = true,
                SendImmediately = true,
                AddUserChatLevel = true,
                AddAdminTag = true
            };

            var result = await _messageSendingService.SendAuctionMessageAsync(
                input.From,null,
                input.Chan,
                input.Message
            );

            return new
            {
                code = result.Success ? 0 : 1,
                data = new
                {
                    messageId = result.MessageId,
                    sequenceNumber = result.SequenceNumber,
                    timestamp = result.Timestamp,
                    message = result.Data
                },
                message = result.Message
            };
        }
    }
}
