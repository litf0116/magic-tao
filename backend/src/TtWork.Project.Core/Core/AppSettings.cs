namespace TtWork.Project.Core;

/// <summary>
/// 应用设置常量定义
/// </summary>
public static class AppSettings
{
    /// <summary>
    /// 版本控制相关设置名称
    /// </summary>
    public static class VersionControl
    {
        /// <summary>
        /// 最新稳定版本号（用于控制拍卖场显示）
        /// 存储格式: YYYYMMDD@主.次.补 (例: 20260224@1.1.21)
        /// </summary>
        public const string LatestStableVersion = "App.VersionControl.LatestStableVersion";
        
        /// <summary>
        /// 拍卖场频道ID（用于过滤）
        /// </summary>
        public const long AuctionChannelId = -1;
        
        /// <summary>
        /// 常驻系统频道ID（所有用户可见）
        /// </summary>
        public static class DemoChannels
        {
            public const long SystemAnnouncement = -10;
            public const long NewbieHelp = -11;
        }
    }
}