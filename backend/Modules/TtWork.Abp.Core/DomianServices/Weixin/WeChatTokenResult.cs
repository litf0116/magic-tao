namespace Hgbs.Extensions.Weixin;

public class WeChatTokenResult : BaseWeChatReulst
{
    public string access_token { get; set; }
    public int expires_in { get; set; }
}