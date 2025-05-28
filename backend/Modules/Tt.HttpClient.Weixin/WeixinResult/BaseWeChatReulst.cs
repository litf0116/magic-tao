using System;

namespace TtWork.HttpClient.Weixin.WeixiinResult
{
    [Serializable]
    public class BaseWeChatReulst
    {
        public int errcode { get; set; }
        public string errmsg { get; set; }
    }
}