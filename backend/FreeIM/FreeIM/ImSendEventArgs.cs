using System;
using System.Collections.Generic;

// public class ImSendEventArgs : EventArgs
namespace FreeIM;

public record ImSendEventArgs  {
    /// <summary>
    /// 发送者的客户端id
    /// </summary>
    public long SenderClientId { get; }

    /// <summary>
    /// 接收者的客户端id
    /// </summary>
    public List<long> ReceiveClientIds { get; } = new();

    /// <summary>
    /// imServer 服务器节点
    /// </summary>
    public string Server { get; }

    /// <summary>
    /// 消息
    /// </summary>
    public object Message { get; }

    /// <summary>
    /// 是否回执
    /// </summary>
    public bool Receipt { get; }

    internal ImSendEventArgs(string server, long senderClientId, object message, bool receipt = false) {
        this.Server = server;
        this.SenderClientId = senderClientId;
        this.Message = message;
        this.Receipt = receipt;
    }
}