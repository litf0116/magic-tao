using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FreeIM;
using TtWork.Project.Services.Messaging.Models;

namespace TtWork.Project.Services.Messaging
{
    /// <summary>
    /// 统一消息发送服务接口
    /// </summary>
    public interface IMessageSendingService
    {
        /// <summary>
        /// 发送频道消息（群聊）
        /// </summary>
        /// <param name="fromUserId">发送者用户ID</param>
        /// <param name="channel">频道名称</param>
        /// <param name="message">消息内容</param>
        /// <param name="options">发送选项</param>
        /// <returns>发送结果</returns>
        Task<SendMessageResult> SendChannelMessageAsync(long fromUserId, string channel, ChatMessage message, MessageSendOptions options = null);

        /// <summary>
        /// 发送私聊消息
        /// </summary>
        /// <param name="fromUserId">发送者用户ID</param>
        /// <param name="toUserId">接收者用户ID</param>
        /// <param name="message">消息内容</param>
        /// <param name="isReceipt">是否需要回执</param>
        /// <param name="options">发送选项</param>
        /// <returns>发送结果</returns>
        Task<SendMessageResult> SendPrivateMessageAsync(long fromUserId, long toUserId, ChatMessage message, bool isReceipt = false, MessageSendOptions options = null);

        /// <summary>
        /// 发送系统频道消息（跳过权限检查）
        /// </summary>
        /// <param name="channel">频道名称</param>
        /// <param name="message">消息内容</param>
        /// <param name="options">发送选项</param>
        /// <returns>发送结果</returns>
        Task<SendMessageResult> SendSystemChannelMessageAsync(string channel, ChatMessage message, MessageSendOptions options = null);

        /// <summary>
        /// 发送系统私聊消息（跳过权限检查）
        /// </summary>
        /// <param name="toUserId">接收者用户ID</param>
        /// <param name="message">消息内容</param>
        /// <param name="options">发送选项</param>
        /// <returns>发送结果</returns>
        Task<SendMessageResult> SendSystemPrivateMessageAsync(long toUserId, ChatMessage message, MessageSendOptions options = null);

        /// <summary>
        /// 批量发送消息
        /// </summary>
        /// <param name="requests">消息发送请求列表</param>
        /// <returns>批量发送结果</returns>
        Task<BatchSendMessageResult> SendBatchMessagesAsync(IEnumerable<MessageSendRequest> requests);

        /// <summary>
        /// 发送拍卖相关消息的便捷方法
        /// </summary>
        /// <param name="fromUserId">发送者用户ID</param>
        /// <param name="toUserId">接收者用户ID（私聊时使用）</param>
        /// <param name="channel">频道名称（频道消息时使用）</param>
        /// <param name="message">消息内容</param>
        /// <param name="isSystemMessage">是否为系统消息</param>
        /// <returns>发送结果</returns>
        Task<SendMessageResult> SendAuctionMessageAsync(long fromUserId, long? toUserId, string channel, ChatMessage message, bool isSystemMessage = false);

        /// <summary>
        /// 编码卡秒消息为AuctionBid类型
        /// </summary>
        /// <param name="kasecMessage">原始卡秒消息</param>
        /// <returns>编码后的消息</returns>
        ChatMessage EncodeKasecMessage(ChatMessage kasecMessage);
    }
} 