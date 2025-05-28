using System;

namespace TtWork.HttpClient.Weixin.Models;

[Serializable]
public class NotifyInputDto {
    public string MchId { get; set; }

    public string RequestBodyString { get; set; }

    public WeChatPayNotificationInput RequestBody { get; set; }

    public NotifyHttpHeaderModel HttpHeader { get; set; }
}

public class NotifyHttpHeaderModel {
    public string SerialNumber { get; set; }

    public string Timestamp { get; set; }

    public string Nonce { get; set; }

    public string Signature { get; set; }

    public NotifyHttpHeaderModel(string serialNumber, string timestamp, string nonce, string signature) {
        SerialNumber = serialNumber;
        Timestamp = timestamp;
        Nonce = nonce;
        Signature = signature;
    }
}