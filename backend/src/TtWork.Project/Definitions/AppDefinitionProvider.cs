using Abp.Localization;
using TtWork.Abp.AppManagement.Apps;

namespace TtWork.Project.Definitions {
    public class ProjectAppProvider : AppDefinitionProvider {
        public override void Define(IAppDefinitionContext context) {
            context.Add(new AppDefinition(ProjectApp.WechatWork,
                    ProjectApp.WechatWork,
                    "企业微信",
                    null,
                    new LocalizableString(ProjectApp.WechatWork, AppConsts.LocalizationSourceName)
                )
            );

            context.Add(new AppDefinition(ProjectApp.pub,
                    ProjectApp.pub,
                    "公众号",
                    null,
                    new LocalizableString(ProjectApp.pub, AppConsts.LocalizationSourceName)
                )
            );

            context.Add(new AppDefinition(ProjectApp.MiniProgram,
                    ProjectApp.MiniProgram,
                    "小程序",
                    null,
                    new LocalizableString(ProjectApp.pub, AppConsts.LocalizationSourceName)
                )
            );
        }
    }

    public static class ProjectApp {
        // ReSharper disable once InconsistentNaming
        public const string WechatWork = "WechatWork";

        public const string pub = "pub";

        public const string MiniProgram = "uniapp";
    }
}