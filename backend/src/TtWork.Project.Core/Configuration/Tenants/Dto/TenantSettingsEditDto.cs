using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Abp.Runtime.Validation;
using Abp.Timing;

namespace TtWork.Project.Core.Configuration.Tenants.Dto
{
    public class TenantSettingsEditDto
    {
        public WeixinSettingsEditDto Weixin { get; set; }

        public OssSettingEditDto Oss { get; set; }

        public ClientSettingEditDto Client { get; set; }

        public NotifySettingEditDto Notify { get; set; }

        public void ValidateHostSettings()
        {
            var validationErrors = new List<ValidationResult>();
            if (Clock.SupportsMultipleTimezone && Weixin == null)
            {
                validationErrors.Add(new ValidationResult("Weixin settings can not be null", new[] { "Weixin" }));
            }

            if (validationErrors.Count > 0)
            {
                throw new AbpValidationException("Method arguments are not valid! See ValidationErrors for details.", validationErrors);
            }
        }
    }
}