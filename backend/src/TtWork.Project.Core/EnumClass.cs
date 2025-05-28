using System.ComponentModel.DataAnnotations;

namespace TtWork.Project.Core {
    public class EnumClass {
        public enum PayType {
            [Display(Name = "微信")] 微信 = 1,
            [Display(Name = "微信扫码")] 微信扫码 = 2,
            [Display(Name = "支付宝")] 支付宝 = 3,
            [Display(Name = "银联")] 银联 = 4,
            [Display(Name = "用户余额")] 用户余额 = 10
        }

        /// <summary>
        /// 文章的状态
        /// </summary>
        public enum CmsContentStatus {
            /// <summary>
            /// 草稿
            /// </summary>
            Draft = 0,

            /// <summary>
            /// 已发布
            /// </summary>
            Published = 7
        }
    }
}