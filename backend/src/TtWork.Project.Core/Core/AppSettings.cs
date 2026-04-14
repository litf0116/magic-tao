namespace TtWork.Project.Core;

public static class AppSettings
{
    public static class VersionControl
    {
        public const string LatestStableVersion = "App.VersionControl.LatestStableVersion";
        public const long AuctionChannelId = -1;
        
        public static class DemoChannels
        {
            public const long SystemAnnouncement = -10;
            public const long NewbieHelp = -11;
        }
    }

    public static class FeatureSwitch
    {
        public const string ShowAuctionMaxVersionMpWeixin = "AppFeatures.ShowAuction.MaxVersion.mp-weixin";
        public const string ShowTradingPostMaxVersionMpWeixin = "AppFeatures.ShowTradingPost.MaxVersion.mp-weixin";
        public const string ShowBannerMaxVersionMpWeixin = "AppFeatures.ShowBanner.MaxVersion.mp-weixin";
    }
}