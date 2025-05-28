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


    public Message(ChatMessage msg) {
        this.Id = msg.id ?? Guid.NewGuid();
        Type = msg.type;
        Chan = msg.chan;
        From = msg.from;
        FromName = msg.fromName;
        Avatar = msg.avatar;
        To = msg.to;
        Time = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;
        Msg = msg.msg;
        Payload = msg.payload.ToJsonString();
        Receipt = msg.receipt;
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