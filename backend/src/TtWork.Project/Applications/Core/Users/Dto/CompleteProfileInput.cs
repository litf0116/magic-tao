using System.ComponentModel.DataAnnotations;
using Abp.Auditing;
using Abp.Authorization.Users;

namespace TtWork.Project.Applications.Core.Users.Dto
{
    /// <summary>
    /// 完善个人信息输入（绑定手机号、设置用户名和密码）
    /// </summary>
    public class CompleteProfileInput
    {
        /// <summary>
        /// 手机号
        /// </summary>
        [Required]
        [StringLength(AbpUserBase.MaxPhoneNumberLength)]
        [Phone]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        [Required]
        [StringLength(AbpUserBase.MaxUserNameLength)]
        public string UserName { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        [Required]
        [StringLength(AbpUserBase.MaxPlainPasswordLength)]
        [DisableAuditing]
        public string Password { get; set; }
    }
}
