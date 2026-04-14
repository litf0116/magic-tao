using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Abp.Domain.Entities.Auditing;
using FluentValidation;

namespace TtWork.Project.Domains;

[Table("T_BidHistory")]
public class BidHistory : CreationAuditedEntity<long> {
    public long AuctionItemId { get; set; } // 拍卖品的唯一标识符
    public int BidPrice { get; set; } // 出价金额
    public DateTime BidTime { get; set; } // 出价时间

    public bool IsRollBack { get; set; } // 是否回滚

    [StringLength(64)] public string BidUserName { get; set; } // 出价人
    [StringLength(256)] public string BidUserAvatar { get; set; } // 出价人头像
    [ForeignKey(nameof(AuctionItemId))] public AuctionItem? AuctionItem { get; set; }
}

[AutoMapFrom(typeof(BidHistory))]
public class BidHistoryDto : EntityDto<long> {
    public long AuctionItemId { get; set; } // 拍卖品的唯一标识符
    public int BidPrice { get; set; } // 出价金额
    public DateTime BidTime { get; set; } // 出价时间
    public string BidUserName { get; set; } // 出价人
    public string BidUserAvatar { get; set; } // 出价人头像
}

[AutoMapTo(typeof(BidHistory))]
public class BidHistoryCreateDto : EntityDto<long> {
    public long AuctionItemId { get; set; } // 拍卖品的唯一标识符
    public int BidPrice { get; set; } // 出价金额
    public string BidUserName { get; set; } // 出价人
    public string BidUserAvatar { get; set; } // 出价人头像


    public DateTime BidTime { get; set; } = DateTime.Now; // 出价时间
}

public class BidHistoryValidator : AbstractValidator<BidHistoryCreateDto> {
    public BidHistoryValidator() {
        RuleFor(x => x.AuctionItemId).GreaterThan(0);
        RuleFor(x => x.BidPrice).GreaterThan(0);
        // BidUserName and BidUserAvatar are set server-side from user cache, not required from client
    }
}