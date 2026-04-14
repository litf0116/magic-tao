using System;

namespace TtWork.Project.Applications.Pays.Dto;

public class PayOrderStatusDto
{
    public string OrderId { get; set; }
    public string OutTradeNo { get; set; }
    public string Status { get; set; }
    public decimal Amount { get; set; }
    public DateTime? PaidTime { get; set; }
    public string TradeNo { get; set; }
    public string Message { get; set; }
}
