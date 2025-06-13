using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Json;
using FreeIM;
using TtWork.Lib.Extensions;

namespace TtWork.Project.Domains;

[Table("T_Message")]
public class Message : Entity<Guid> {
    public Message() {
    }

    public Message(ChatMessage msg, long sequenceNumber = 0) {
        this.Id = msg.id ?? Guid.NewGuid();
        Type = msg.type;
        Chan = msg.chan;
        From = msg.from;
        FromName = msg.fromName;
        Avatar = msg.avatar;
        To = msg.to;
        // 关键修改：完全忽略客户端时间戳，统一使用服务端时间
        // 这确保了所有消息（PC端、小程序端）都有一致的时间基准
        Time = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;
        Msg = msg.msg;
        Payload = msg.payload.ToJsonString();
        Receipt = msg.receipt;
        // 设置序列号，如果传入0则表示需要生成
        SequenceNumber = sequenceNumber;
    }

    public ChatMessageType Type { get; set; }
    [StringLength(64)] public string Chan { get; set; }
    public long From { get; set; }
    [StringLength(64)] public string FromName { get; set; }

    public bool FromAdmin { get; set; }

    [StringLength(32)] public string FromTag { get; set; }

    [StringLength(32)] public string TagClass { get; set; } = "";
    [StringLength(128)] public string Avatar { get; set; }
    public long? To { get; set; }
    public long Time { get; protected set; }
    [StringLength(2048)] public string Msg { get; set; }
    public string Payload { get; set; } = "{}";
    [StringLength(64)] public string Receipt { get; set; }

    [StringLength(64)] public string Ip { get; set; }
    
    // 新增：消息序列号，用于确保消息顺序
    public long SequenceNumber { get; set; }
}

[Table("T_UserFriend")]
public class UserFriend : Entity<int> {
    [NotMapped] public override int Id { get; set; }
    public long UserId { get; set; }
    public long FriendId { get; set; }

    // 备注
    [StringLength(64)] public string Remark { get; private set; }

    public bool Status { get; set; } //true is accept .false is waiting for accept
}