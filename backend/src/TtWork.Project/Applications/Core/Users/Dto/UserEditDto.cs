using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;
using Abp.Auditing;
using Abp.Authorization.Users;
using Abp.AutoMapper;
using Abp.Domain.Entities;
using TtWork.Abp.Authorization.Users;

namespace TtWork.Project.Applications.Core.Users.Dto {
    //Mapped to/from User in CustomDtoMapper
    [AutoMapTo(typeof(User))]
    [AutoMapFrom(typeof(User))]
    public class UserEditDto : EntityDto<long>, IPassivable {
        /// <summary>
        /// Set null to create a new user. Set user's Id to update a user
        /// </summary>
        public long Id { get; set; }

        [Required]
        [StringLength(AbpUserBase.MaxUserNameLength)]
        public string UserName { get; set; }

        // [Required]
        // [EmailAddress]
        [StringLength(AbpUserBase.MaxEmailAddressLength)]
        public string EmailAddress { get; set; }

        [Required]
        [StringLength(AbpUserBase.MaxNameLength)]
        public string Name { get; set; }

        [StringLength(AbpUserBase.MaxSurnameLength)]
        public string Surname { get; set; }

        public string HeadImgUrl { get; set; }

        [StringLength(AbpUserBase.MaxPhoneNumberLength)]
        public string PhoneNumber { get; set; }

        // Not used "Required" attribute since empty value is used to 'not change password'
        [StringLength(AbpUserBase.MaxPlainPasswordLength)]
        [DisableAuditing]
        public string Password { get; set; }

        public bool IsActive { get; set; } = true;

        /// <summary>
        /// QQ
        /// </summary>
        [StringLength(AbpUserBase.MaxPhoneNumberLength)]
        public string Qq { get; set; }

        /// <summary>
        /// 微信号
        /// </summary>
        [StringLength(AbpUserBase.MaxPhoneNumberLength)]
        public string Wx { get; set; }
        /// <summary>
        /// 保证金
        /// </summary>
        public decimal DepositBalance { get; set; }
    }
}