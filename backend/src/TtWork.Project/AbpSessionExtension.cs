using System.Linq;
using Abp.Runtime.Session;

namespace TtWork
{
    public static class AbpSessionExtension
    {
        public static string GetCurrentOu(this IAbpSession session)
        {
            return GetClaimValue("Abp.OrganizationUnitId");
        }

        private static string GetClaimValue(string claimType)
        {
            var claimsPrincipal = DefaultPrincipalAccessor.Instance.Principal;

            var claim = claimsPrincipal?.Claims.FirstOrDefault(c => c.Type == claimType);
            if (string.IsNullOrEmpty(claim?.Value))
                return null;

            return claim.Value;
        }
    }
}