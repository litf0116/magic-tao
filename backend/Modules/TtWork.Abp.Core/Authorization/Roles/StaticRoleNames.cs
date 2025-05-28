namespace TtWork.Abp.Authorization.Roles {
    public static class StaticRoleNames {
        public static class Host {
            public const string Admin = "Admin";
        }

        public static class Tenants {
            public const string Admin = "Admin";
            public const string Admin_CN = "系统管理员";

            public const string Organize = "Organize";

            public const string User = "User";
        }
    }
}