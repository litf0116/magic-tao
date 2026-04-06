using System.ComponentModel.DataAnnotations;
using Abp.Auditing;
using Abp.Authorization.Users;
using Abp.AutoMapper;
using Abp.Runtime.Validation;
using TtWork.Abp.Authorization.Users;

namespace TtWork.Project.Applications.Core.Users.Dto {
    /// <summary>
    /// 用户创建Dto
    /// </summary>
    [AutoMapTo(typeof(User))]
    public class CreateUserDto {
        [Required]
        [StringLength(AbpUserBase.MaxUserNameLength)]
        public string UserName { get; set; }

        [Required]
        [StringLength(64)]
        public string Name { get; set; }

        [StringLength(64)]
        public string Surname { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(AbpUserBase.MaxEmailAddressLength)]
        public string EmailAddress { get; set; }

        [StringLength(32)]
        public string PhoneNumber { get; set; }

        [Required]
        [StringLength(AbpUserBase.MaxPlainPasswordLength)]
        [DisableAuditing]
        public string Password { get; set; }

        public bool IsActive { get; set; } = true;
    }
}