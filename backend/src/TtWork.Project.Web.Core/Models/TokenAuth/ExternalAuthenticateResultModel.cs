namespace TtWork.Project.Models.TokenAuth
{
    public class ExternalAuthenticateResultModel
    {
        public string AccessToken { get; set; }

        public string EncryptedAccessToken { get; set; }

        public int ExpireInSeconds { get; set; }

        public bool WaitingForActivation { get; set; }

        public string ReturnUrl { get; set; }

        public string RefreshToken { get; set; }

        public int RefreshTokenExpireInSeconds { get; set; }
        
        public object Extension { get; set; }
        
        public string[] RoleNames { get; set; }

        /// <summary>
        /// 是否需要完善个人信息
        /// </summary>
        public bool NeedProfileCompletion { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// 是否需要绑定手机号（微信登录后必须绑定）
        /// </summary>
        public bool NeedPhoneBinding { get; set; }

        /// <summary>
        /// 临时Token，用于绑定手机号流程
        /// </summary>
        public string BindToken { get; set; }

        /// <summary>
        /// 用户名（用于绑定页显示）
        /// </summary>
        public string UserName { get; set; }
    }
}
