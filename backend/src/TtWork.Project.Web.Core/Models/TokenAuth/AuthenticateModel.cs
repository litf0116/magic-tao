using System.ComponentModel.DataAnnotations;
using Abp.Auditing;
using Abp.Authorization.Users;

namespace TtWork.Project.Web.Core.Models.TokenAuth {
    public class AuthenticateModel {
        [Required]
        [StringLength(AbpUserBase.MaxEmailAddressLength)]
        public string UserNameOrEmailAddress { get; set; }

        [Required]
        [StringLength(AbpUserBase.MaxPlainPasswordLength)]
        public string Password { get; set; }

        public bool RememberClient { get; set; }

        public bool? SingleSignIn { get; set; }

        public string ReturnUrl { get; set; }

        [DisableAuditing] public string CaptchaResponse { get; set; }
    }
}