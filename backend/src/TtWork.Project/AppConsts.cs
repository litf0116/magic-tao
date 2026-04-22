using System;

namespace TtWork.Project {
    public class AppConsts {
        public const string LocalizationSourceName = "Project";
        public const string DbTablePrefix = "T_";
        public const string DbSchema = null;

        /// <summary>
        /// Default pass phrase for SimpleStringCipher decrypt/encrypt operations
        /// </summary>
        public const string DefaultPassPhrase = "gsKxGZ012HLL3MI5";

        public const int MaxPageSize = 1000;
        public const int DefaultPageSize = 10;

        public static TimeSpan AccessTokenExpiration = TimeSpan.FromDays(7);
        public static TimeSpan RefreshTokenExpiration = TimeSpan.FromDays(7);

        public const string TokenValidityKey = "token_validity_key";
        public const string RefreshTokenValidityKey = "refresh_token_validity_key";
        public const string SecurityStampKey = "AspNet.Identity.SecurityStamp";

        public const string TokenType = "token_type";
        public static string UserIdentifier = "user_identifier";


        public const string SensitiveWordsCacheKey = "SensitiveWords";

        public const string UserBanText = "您的账号严重违反了【魔力淘】平台规定，予以封号处理，解决封号问题请联系拍卖师老淡QQ：383875411";
        public const string UserDefaultAvatar = "https://image.molitao.top/avater.png";


        public const decimal 保证金 = 51m;


        public static class CacheKeys {
            public const string MyCount = "MyCount-{0}";
        }
        
        
        public static class WorkWxKeys {
#if DEBUG
            //测试用群
            public const string wxworkid = "380d770e-9172-4232-b911-7df897aab0e7";
#else
    //咔嚓一下企业微信
    public const string wxworkid = "296d90d8-82f7-4280-ab38-3d7a23c767d3";
#endif
            public const string 支付成功通知群 = wxworkid;
        }
    }
}