namespace TtWork.Project.Web.Models.TokenAuth {
    public class WeChatMiniProgramAuthenticateModel {
        
        public string code { get; set; }

        /// <summary>
        /// 解密Userinfo使用
        /// </summary>
        public string encryptedData { get; set; }

        public string iv { get; set; }

        public string session_key { get; set; }

        public string openid { get; set; }

        public string unionid { get; set; }

        public string appid { get; set; }
    }
}