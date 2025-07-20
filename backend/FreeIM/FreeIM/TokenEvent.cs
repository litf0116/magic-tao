using System.Collections.Generic;

namespace FreeIM {
    public record TokenEvent(long ClientId, string Ip) {
        public long ClientId { get; set; } = ClientId;
        public string Ip { get; set; } = Ip;
    }

    public record MessageEvent(
        long SenderClientId,
        List<long> ReceiverClientIds,
        ChatMessage Message,
        bool Receipt = false) {
        public long SenderClientId { get; set; } = SenderClientId;
        public List<long> ReceiverClientIds { get; set; } = ReceiverClientIds;
        public ChatMessage Message { get; set; } = Message;
        public bool Receipt { get; set; } = Receipt;
    }

    public enum ChatMessageType {
        Error = -1,
        Text = 1,
        Image = 2,
        File = 3,
        Welcome = 100, //进入房间
        Goodbye = 101, //踢出房间
        BanUser = 102, //禁言用户
        Backout = 110, //撤回
        Receipt = 10,

        AuctionStart = 1000,
        AuctionBid = 1002,
        AuctionEnd = 1010,
        AuctionDeal = 1011, //拍卖成交通知

        // 卡秒状态
        KasecStatusChanged = 2000,
    }

    public enum ChatMessageStatus {
        Sending,
        Fail,
        Success
    }
}