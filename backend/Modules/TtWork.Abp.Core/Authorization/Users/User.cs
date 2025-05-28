using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Authorization.Users;
using Abp.Extensions;
using Abp.Timing;
using Microsoft.EntityFrameworkCore;

namespace TtWork.Abp.Authorization.Users {
    public class User : AbpUser<User> {
        public const string DefaultPassword = "4321@reEq*#";
        [StringLength(256)] public virtual string HeadImgUrl { get; set; }

        public virtual List<UserOrganizationUnit> OrganizationUnits { get; set; }
        // public DateTime? SignInTokenExpireTimeUtc { get; set; }
        // public string SignInToken { get; set; }

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
        /// 帐户余额
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Balance { get; set; }

        /// <summary>
        /// 保证金
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal DepositBalance { get; set; }


        public static string CreateRandomPassword() {
            return Guid.NewGuid().ToString("N").Truncate(16);
        }

        public static User CreateTenantAdminUser(int tenantId, string emailAddress) {
            var user = new User {
                TenantId = tenantId,
                UserName = AdminUserName,
                Name = AdminUserName,
                Surname = AdminUserName,
                EmailAddress = emailAddress,
                Roles = new List<UserRole>()
            };

            user.SetNormalizedNames();

            return user;
        }


        public override void SetNewPasswordResetCode() {
            /* This reset code is intentionally kept short.
             * It should be short and easy to enter in a mobile application, where user can not click a link.
             */
            PasswordResetCode = Guid.NewGuid().ToString("N").Truncate(10).ToUpperInvariant();
        }

        public void Unlock() {
            AccessFailedCount = 0;
            LockoutEndDateUtc = null;
        }

        // public void SetSignInToken() {
        //     SignInToken = Guid.NewGuid().ToString();
        //     SignInTokenExpireTimeUtc = Clock.Now.AddMinutes(1).ToUniversalTime();
        // }
    }
}