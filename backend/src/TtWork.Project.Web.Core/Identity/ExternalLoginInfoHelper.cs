using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Abp.Authorization.Users;
using Abp.Extensions;
using TtWork.Abp.Authorization.Users;
using TtWork.Abp.Core.MultiTenancy;
using TtWork.Lib;
using TtWork.Project.Authentication.External;
using TtWork.Project.Web.Authentication.External;

namespace TtWork.Project.Web.Core.Identity {
    public static class ExternalLoginInfoHelper {
        public static void AddWeixinClaim(this AbpLoginResult<Tenant, User> loginResult,
            ExternalAuthUserInfo externalUser) {
            #region 把微信的openid和unionid加入到jwt token

            if (!externalUser.UserName.IsNullOrEmptyOrWhiteSpace())
                loginResult.Identity.AddClaim(new Claim("usename", externalUser.UserName));

            #endregion
        }


        public static (string name, string surname) GetNameAndSurnameFromClaims(List<Claim> claims) {
            string name = null;
            string surname = null;

            var givennameClaim = claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName);
            if (givennameClaim != null && !givennameClaim.Value.IsNullOrEmpty()) {
                name = givennameClaim.Value;
            }

            var surnameClaim = claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname);
            if (surnameClaim != null && !surnameClaim.Value.IsNullOrEmpty()) {
                surname = surnameClaim.Value;
            }

            if (name == null || surname == null) {
                var nameClaim = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
                if (nameClaim != null) {
                    var nameSurName = nameClaim.Value;
                    if (!nameSurName.IsNullOrEmpty()) {
                        var lastSpaceIndex = nameSurName.LastIndexOf(' ');
                        if (lastSpaceIndex < 1 || lastSpaceIndex > (nameSurName.Length - 2)) {
                            name = surname = nameSurName;
                        }
                        else {
                            name = nameSurName.Substring(0, lastSpaceIndex);
                            surname = nameSurName.Substring(lastSpaceIndex);
                        }
                    }
                }
            }

            return (name, surname);
        }
    }
}