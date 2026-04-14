using System.Linq;
using Microsoft.EntityFrameworkCore;
using Abp.Configuration;
using Abp.Localization;
using Abp.MultiTenancy;
using Abp.Net.Mail;
using TtWork.Project.Core;
using TtWork.Project.EntityFrameworkCore;

namespace TtWork.Project.EntityFrameworkCore.Seed.Host
{
    public class DefaultSettingsCreator
    {
        private readonly AbpDbContext _context;

        public DefaultSettingsCreator(AbpDbContext context)
        {
            _context = context;
        }

        public void Create()
        {
            int? tenantId = null;

            if (CoreConsts.MultiTenancyEnabled == false)
            {
               // tenantId = MultiTenancyConsts.DefaultTenantId;
            }

            // Emailing
            AddSettingIfNotExists(EmailSettingNames.DefaultFromAddress, "admin@mydomain.com", tenantId);
            AddSettingIfNotExists(EmailSettingNames.DefaultFromDisplayName, "mydomain.com mailer", tenantId);

            // Languages
            AddSettingIfNotExists(LocalizationSettingNames.DefaultLanguage, "zh-Hans", tenantId);

            // Version Control
            AddSettingIfNotExists(
                TtWork.Project.Core.AppSettings.VersionControl.LatestStableVersion,
                "20260224@1.1.21",
                tenantId
            );

            // Feature Switch
            AddSettingIfNotExists(
                TtWork.Project.Core.AppSettings.FeatureSwitch.ShowAuctionMaxVersionMpWeixin,
                "20260401@1.1.9",
                tenantId
            );

            AddSettingIfNotExists(
                TtWork.Project.Core.AppSettings.FeatureSwitch.ShowTradingPostMaxVersionMpWeixin,
                "20260401@1.1.9",
                tenantId
            );

            AddSettingIfNotExists(
                TtWork.Project.Core.AppSettings.FeatureSwitch.ShowBannerMaxVersionMpWeixin,
                "20260401@1.1.9",
                tenantId
            );
        }

        private void AddSettingIfNotExists(string name, string value, int? tenantId = null)
        {
            if (_context.Settings.IgnoreQueryFilters().Any(s => s.Name == name && s.TenantId == tenantId && s.UserId == null))
            {
                return;
            }

            _context.Settings.Add(new Setting(tenantId, null, name, value));
            _context.SaveChanges();
        }
    }
}
