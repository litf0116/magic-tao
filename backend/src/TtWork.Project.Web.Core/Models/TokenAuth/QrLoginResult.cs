namespace TtWork.Project.Web.Core.Models.TokenAuth
{
    /// <summary>
    /// 扫码登录结果
    /// </summary>
    public class QrLoginResult
    {
        /// <summary>
        /// JWT访问令牌
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// 是否需要完善个人信息
        /// </summary>
        public bool NeedProfileCompletion { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        public long UserId { get; set; }
    }
}