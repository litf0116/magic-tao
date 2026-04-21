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
        public const string ReviewVersionMpWeixin = "AppFeatures.ReviewVersion.mp-weixin";
        public const string ReviewVersionAppPlus = "AppFeatures.ReviewVersion.app-plus";
        public const string ReviewVersionH5 = "AppFeatures.ReviewVersion.h5";
    }
}