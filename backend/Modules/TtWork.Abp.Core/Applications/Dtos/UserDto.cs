using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;
using Abp.Authorization.Users;
using Abp.AutoMapper;
using TtWork.Abp.Authorization.Users;
using TtWork.Abp.Core.Authorization.Users;

namespace TtWork.Abp.Applications.Dtos
{
    [AutoMapFrom(typeof(User))]
    public class UserDto : EntityDto<long>
    {
        [Required]
        [StringLength(AbpUserBase.MaxUserNameLength)]
        public string UserName { get; set; }

        [Required]
        [StringLength(AbpUserBase.MaxNameLength)]
        public string Name { get; set; }

        [StringLength(AbpUserBase.MaxSurnameLength)]
        public string Surname { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(AbpUserBase.MaxEmailAddressLength)]
        public string EmailAddress { get; set; }

        public bool IsActive { get; set; }

        public string FullName { get; set; }

        public DateTime? LastLoginTime { get; set; }

        public DateTime CreationTime { get; set; }
        public string[] RoleNames { get; set; }

        public string PhoneNumber { get; set; }

        public string HeadImgUrl { get; set; }

        public int FromClient { get; set; }

        public List<string> Permissions { get; set; }

        public string Qq { get; set; }

        public string Wx { get; set; }
        /// <summary>
        /// 帐户余额
        /// </summary>
        public decimal Balance { get; set; }
        /// <summary>
        /// 保证金
        /// </summary>
        public decimal DepositBalance { get; set; }
        
        /// <summary>
        /// 是否跳过完善个人信息引导
        /// </summary>
        public bool SkipProfileCompletion { get; set; }
        
        /// <summary>
        /// 累计拍卖金额（来自用户群聊等级表）
        /// </summary>
        public decimal CumulativeAmount { get; set; }
    }
}