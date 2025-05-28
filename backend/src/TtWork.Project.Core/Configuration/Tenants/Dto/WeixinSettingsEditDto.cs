using Abp.Auditing;

namespace TtWork.Project.Core.Configuration.Tenants.Dto
{
    public class WeixinSettingsEditDto
    {
        public string AppId { get; set; }

        [DisableAuditing] public string AppSecret { get; set; }

        public string Mini_AppId { get; set; }

        [DisableAuditing] public string Mini_AppSecret { get; set; }

        public string Pay_MchId { get; set; }

        [DisableAuditing] public string Pay_Key { get; set; }

        public string Pay_Notify { get; set; }

        public string TenPay_AppId { get; set; }

        public string TenPay_AppSecret { get; set; }

        public string TenPay_RefundAccount { get; set; }
    }
}