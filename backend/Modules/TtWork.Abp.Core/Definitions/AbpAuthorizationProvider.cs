using Abp.Authorization;
using Abp.Localization;

namespace TtWork.Abp.Definitions {
    public class AbpAuthorizationProvider : AuthorizationProvider {
        public override void SetPermissions(IPermissionDefinitionContext context) {
            var pages = context.GetPermissionOrNull(AppPermissions.Pages.Default) ??
                        context.CreatePermission(AppPermissions.Pages.Default, L("Permission:Pages"));

            var chatManager = pages.CreateChildPermission(AppPermissions.Pages.ChatManager,
                L("Permission:ChatManager"));

            var auctionManager = pages.CreateChildPermission(AppPermissions.Pages.AuctionManager,
                L("Permission:AuctionManager"));

            var administration = pages.CreateChildPermission(AppPermissions.Administration,
                L("Permission:Administration"));
        }

        private static ILocalizableString L(string name) {
            return new LocalizableString(name, Consts.LocalizationSourceName);
        }
    }

    /// <summary>
    /// 应用权限
    /// </summary>
    public static class AppPermissions {
        /// <summary>
        /// 系统管理员
        /// </summary>
        public const string Administration = "Pages.Administration";

        /// <summary>
        /// 默认
        /// </summary>
        public class Pages {
            /// <summary>
            /// 默认
            /// </summary>
            public const string Default = "Pages";


            /// <summary>
            /// 聊天管理
            /// </summary>
            public const string ChatManager = "Pages.Chat.Manager";

            /// <summary>
            /// 出价
            /// </summary>
            public const string Auction = "Pages.Auction.Auction";

            /// <summary>
            /// 竞拍管理
            /// </summary>
            public const string AuctionManager = "Pages.Auction.Manager";
        }
    }
}