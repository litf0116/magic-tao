namespace TtWork.HttpClient.Weixin.Models;

public class TenPayResult {
    public string return_code { get; set; }
    public string return_msg { get; set; }
}

public class TransfersResult : TenPayResult {
    public string mch_appid { get; set; }
    public string mchid { get; set; }
    public string result_code { get; set; }
    public string partner_trade_no { get; set; }
    public string payment_no { get; set; }
    public string payment_time { get; set; }
    public string err_code { get; set; }
    public string err_code_des { get; set; }

    public bool IsSuccess => result_code == "SUCCESS" && return_code == "SUCCESS";
}