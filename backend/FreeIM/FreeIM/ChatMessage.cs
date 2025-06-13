using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FreeIM;

public record ChatMessage
{
    public Guid? id { get; set; } = Guid.NewGuid();

    [JsonConverter(typeof(StringEnumConverter))]
    public ChatMessageType type { get; set; } = ChatMessageType.Text;

    [JsonConverter(typeof(StringEnumConverter))]
    public ChatMessageStatus status { get; set; }

    public string chan { get; set; }
    public long from { get; set; }

    public string fromName { get; set; }
    public bool fromAdmin { get; set; }

    public string fromTag { get; set; }

    public string tagClass { get; set; } = "";

    public string avatar { get; set; }
    public long? to { get; set; }

    public long time { get; set; } = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;

    public string msg { get; set; }

    public object payload { get; set; } = new();
    public string receipt { get; set; }

    public dynamic userChatLevel { get; set; }
    
    // 新增：消息序列号，用于确保消息顺序
    public long sequenceNumber { get; set; } = 0;
    
    public long GetNowTime()
    {
        return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;
    }
}