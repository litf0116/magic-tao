using Abp.AspNetCore.Mvc.Controllers;
using Abp.IdentityFramework;
using Microsoft.AspNetCore.Identity;
using TtWork.Project.Core;

namespace TtWork.Project.Controllers
{
    public abstract class AbpControllerBase: AbpController
    {
        protected AbpControllerBase()
        {
            LocalizationSourceName = CoreConsts.LocalizationSourceName;
        }

        protected void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }
    }
}
