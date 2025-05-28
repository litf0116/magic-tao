using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Abp.Domain.Entities.Auditing;
using Abp.Json;
using Abp.UI;
using FluentValidation;
using JetBrains.Annotations;
using TtWork.Lib;

namespace TtWork.Project.Domains;

// [Flags]
public enum AuctionStatusEnum
{
    草稿 = 0,
    上架 = 1,
    拍卖中 = 2,
    已成交 = 4,
    交易成功 = 8,
    卖家失约 = 16,
    买家失约 = 32,
    交易关闭 = 128,
}

[Table("T_AuctionItem")]
public class AuctionItem : FullAuditedAggregateRoot<long>
{
    [StringLength(128)] public string Name { get; set; }
    public AuctionStatusEnum Status { get; private set; }
    [StringLength(256)] public string ImageUrl { get; set; }
    public string Description { get; set; }
    public int StartingPrice { get; set; }
    /// <summary>
    /// 当前出价
    /// </summary>
    public int? CurrentPrice { get; set; }
    /// <summary>
    /// 当前出价人
    /// </summary>
    public long? CurrentPriceUserId { get; set; }
    /// <summary>
    /// 当前出价人
    /// </summary>
    [StringLength(64)] public string CurrentPriceUserName { get; set; }
    /// <summary>
    /// 成交价
    /// </summary>
    public int? FinalPrice { get; private set; }
    /// <summary>
    /// 成交时间
    /// </summary>
    public DateTime? DealTime { get; private set; }
    /// <summary>
    /// 成交人
    /// </summary>
    public long? DealUserId { get; private set; }
    /// <summary>
    /// 成交人
    /// </summary>
    [StringLength(64)] public string DealUserName { get; private set; }

    [StringLength(256)] public string SellerInfo { get; set; }
    /// <summary>
    /// 出售人
    /// </summary>
    public long? SellerId { get; set; }

    public int Order { get; set; }

    public List<BidHistory> BidHistories { get; set; }


    public void StartAuction()
    {
        Status = AuctionStatusEnum.拍卖中;
    }

    public void SetDeal()
    {
        Status = AuctionStatusEnum.已成交;

        FinalPrice = CurrentPrice;
        DealTime = DateTime.Now;
        DealUserId = CurrentPriceUserId;
        DealUserName = CurrentPriceUserName;
    }

    public void SetBid(int inputBidPrice, long abpSessionUserId, string inputBidUserName)
    {
        CurrentPrice = inputBidPrice;
        CurrentPriceUserId = abpSessionUserId;
        CurrentPriceUserName = inputBidUserName;
    }

    public void Back()
    {
        Status = AuctionStatusEnum.上架;
    }

    public void RollBack([CanBeNull] BidHistory previousBid)
    {
        CurrentPrice = previousBid?.BidPrice;
        CurrentPriceUserId = previousBid?.CreatorUserId;
        CurrentPriceUserName = previousBid?.BidUserName;
    }
}

public record PriceRecord(int? CurrentPrice, long? CurrentPriceUserId, string CurrentPriceUserName);

[AutoMapFrom(typeof(AuctionItem))]
public class AuctionItemDto : EntityDto<long>
{
    public string Name { get; set; }
    public AuctionStatusEnum Status { get; set; }
    public string ImageUrl { get; set; }
    public string Description { get; set; }
    public int StartingPrice { get; set; }
    /// <summary>
    /// 当前价格
    /// </summary>
    public int? CurrentPrice { get; set; }
    /// <summary>
    /// 当前出价人编号
    /// </summary>
    public long? CurrentPriceUserId { get; set; }
    /// <summary>
    /// /当前出价人
    /// </summary>
    public string CurrentPriceUserName { get; set; }
    /// <summary>
    /// 当前出价时间
    /// </summary>
    public DateTime CurrentPriceTime { get; set; }
    /// <summary>
    /// 倒计时时间
    /// </summary>
    public DateTime UseCountdownTime { get; set; }
    public int? FinalPrice { get; set; }
    public DateTime? DealTime { get; set; }
    public long? DealUserId { get; set; }
    public string DealUserName { get; set; } //成交人

    public string SellerInfo { get; set; }

    public long? SellerId { get; set; }  //出售人
    //排序
    public int Order { get; set; }

    //前端使用
    public string ToUserMsg { get; set; }
    public string DealUserAvatar { get; set; } //成交人头像
}

[AutoMapFrom(typeof(AuctionItem))]
[AutoMapTo(typeof(AuctionItem))]
public class AuctionItemCreateOrUpdateDto : EntityDto<long>
{
    public string Name { get; set; }
    public int Status { get; set; } = 1;
    public string ImageUrl { get; set; }
    public string Description { get; set; }
    public int StartingPrice { get; set; }

    public string SellerInfo { get; set; }
    public int Order { get; set; }

    public long? SellerId { get; set; }  //出售人
}

public class AuctionItemCreateOrUpdateDtoValidator : AbstractValidator<AuctionItemCreateOrUpdateDto>
{
    public AuctionItemCreateOrUpdateDtoValidator()
    {
        RuleFor(x => x).Must(x => !x.Name.IsNullOrEmptyOrWhiteSpace())
            .WithMessage("标题不能为空");
        RuleFor(x => x).Must(x => !x.ImageUrl.IsNullOrEmptyOrWhiteSpace())
            .WithMessage("图片不能为空");
        RuleFor(x => x).Must(x => !x.SellerInfo.IsNullOrEmptyOrWhiteSpace())
            .WithMessage("卖家信息不能为空");
    }
}