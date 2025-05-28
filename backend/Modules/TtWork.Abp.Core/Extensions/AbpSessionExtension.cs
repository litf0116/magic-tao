using System;
using System.Linq;
using Abp.Dependency;
using Abp.Runtime.Session;
using Microsoft.AspNetCore.Http;
using TtWork.Lib;

namespace TtWork.Abp.Extensions
{
    public static class AbpSessionExtension
    {
        public static string Get_AppName(this IHttpContextAccessor httpContextAccessor) {
            if (httpContextAccessor?.HttpContext != null)
                return httpContextAccessor?.HttpContext.Request.Headers["AppName"].FirstOrDefault();
            else throw new Exception("HttpContext is Null");
        }

        public static string Get_openid(this IAbpSession session)
        {
            return GetClaimValue("openid");
        }

        public static string Get_unionid(this IAbpSession session)
        {
            return GetClaimValue("unionid");
        }

        public static string Get_nickname(this IAbpSession session)
        {
            return GetClaimValue("nickname");
        }
        
        public static string Get_headimgurl(this IAbpSession session)
        {
            return GetClaimValue("headimgurl");
        }
        
        public static string Get_Phone(this IAbpSession session)
        {
            return GetClaimValue("Phone");
        }

        /// <summary>
        /// 获取用户当前所选择的门店
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        public static long? Get_OrganizationUnitId(this IAbpSession session)
        {
            try
            {
                var s = IocManager.Instance.Resolve<IHttpContextAccessor>();
                var claim = s?.HttpContext.Request.Headers["Abp.OrganizationUnitId"].FirstOrDefault();

                if (!claim.IsNullOrEmptyOrWhiteSpace())
                    if (long.TryParse(claim, out var result))
                        return result;
            }
            catch (Exception)
            {
                // ignored
            }

            return null;
        }

        private static string GetClaimValue(string claimType)
        {
            var s = IocManager.Instance.Resolve<IPrincipalAccessor>();
            var claim = s?.Principal?.Claims.FirstOrDefault(c => c.Type == claimType);
            return string.IsNullOrEmpty(claim?.Value) ? null : claim.Value;
        }
    }
}