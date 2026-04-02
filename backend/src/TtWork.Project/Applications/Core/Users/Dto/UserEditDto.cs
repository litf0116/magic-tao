using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;
using Abp.Auditing;
using Abp.Authorization.Users;
using Abp.AutoMapper;
using Abp.Domain.Entities;
using TtWork.Abp.Authorization.Users;

namespace TtWork.Project.Applications.Core.Users.Dto
{
    //Mapped to/from User in CustomDtoMapper
    [AutoMapTo(typeof(User))]
    [AutoMapFrom(typeof(User))]
    public class UserEditDto : EntityDto<long>
    {
        /// <summary>
        /// Set null to create a new user. Set user's Id to update a user
        /// </summary>
        public new long Id { get; set; }

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

        /// <summary>
        /// 用户状态（客户端不可修改，后端忽略此字段）
        /// </summary>
        public bool? IsActive { get; set; }

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
        /// 保证金（客户端不可修改，后端忽略此字段）
        /// </summary>
        public decimal? DepositBalance { get; set; }
    }
}