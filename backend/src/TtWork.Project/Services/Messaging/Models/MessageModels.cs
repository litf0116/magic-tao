using System;
using System.Collections.Generic;
using FreeIM;

namespace TtWork.Project.Services.Messaging.Models
{
    /// <summary>
    /// 消息发送选项
    /// </summary>
    public class MessageSendOptions
    {
        /// <summary>
        /// 跳过权限检查（用于系统消息）
        /// </summary>
        public bool SkipPermissionCheck { get; set; } = false;

        /// <summary>
        /// 跳过敏感词检查
        /// </summary>
        public bool SkipSensitiveWordCheck { get; set; } = false;

        /// <summary>
        /// 是否持久化到数据库
        /// </summary>
        public bool PersistToDatabase { get; set; } = true;

        /// <summary>
        /// 是否立即发送
        /// </summary>
        public bool SendImmediately { get; set; } = true;

        /// <summary>
        /// 是否添加用户群聊等级信息
        /// </summary>
        public bool AddUserChatLevel { get; set; } = true;

        /// <summary>
        /// 是否添加管理员标签
        /// </summary>
        public bool AddAdminTag { get; set; } = true;
    }

    /// <summary>
    /// 消息发送请求
    /// </summary>
    public class MessageSendRequest
    {
        public long FromUserId { get; set; }
        public long? ToUserId { get; set; }
        public string Channel { get; set; }
        public ChatMessage Message { get; set; }
        public bool IsReceipt { get; set; } = false;
        public MessageSendOptions Options { get; set; } = new();
    }

    /// <summary>
    /// 消息发送结果
    /// </summary>
    public class SendMessageResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public Guid? MessageId { get; set; }
        public long? SequenceNumber { get; set; }
        public DateTime? Timestamp { get; set; }
        public object Data { get; set; }

        public static SendMessageResult CreateSuccess(Guid? messageId = null, long? sequenceNumber = null, DateTime? timestamp = null, object data = null)
        {
            return new SendMessageResult
            {
                Success = true,
                Message = "发送成功",
                MessageId = messageId,
                SequenceNumber = sequenceNumber,
                Timestamp = timestamp,
                Data = data
            };
        }

        public static SendMessageResult CreateFailure(string message)
        {
            return new SendMessageResult
            {
                Success = false,
                Message = message
            };
        }
    }

    /// <summary>
    /// 批量消息发送结果
    /// </summary>
    public class BatchSendMessageResult
    {
        public int TotalCount { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<SendMessageResult> Results { get; set; } = new();
        public List<string> Errors { get; set; } = new();
    }
} 