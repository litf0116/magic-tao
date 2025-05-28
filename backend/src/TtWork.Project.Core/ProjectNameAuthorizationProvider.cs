using Abp.Authorization;
using Abp.Localization;

namespace TtWork.Project.Core {
    public class ProjectNameAuthorizationProvider : AuthorizationProvider {
        public override void SetPermissions(IPermissionDefinitionContext context) {
            // var pages = context.GetPermissionOrNull(AppPermissions.Pages.Default) ?? context.CreatePermission(AppPermissions.Pages.Default, L("Pages"));
        }

        private static ILocalizableString L(string name) {
            return new LocalizableString(name, CoreConsts.LocalizationSourceName);
        }
    }
}