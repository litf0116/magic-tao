using System.Collections.Generic;
using TtWork.Abp.DomianServices;

namespace TtWork.Project.Web.Authentication.External {
    /// <summary>
    /// <see cref="https://developers.weixin.qq.com/miniprogram/dev/framework/open-ability/getPhoneNumber.html"/>
    /// </summary>
    public class GetPhoneInfo {
        public string phoneNumber { get; set; }

        public string purePhoneNumber { get; set; }

        public string countryCode { get; set; }
    }


    public class ExternalAuthUserInfo {
        public long Id { get; set; }
        public string ProviderKey { get; set; }

        public string Name { get; set; }

        public string EmailAddress { get; set; }

        public string Surname { get; set; }

        public string Provider { get; set; }

        public string ProviderName { get; set; }

        public string UserName { get; set; }

        public string PhoneNumber { get; set; }

        public string Password { get; set; }

        public string HeadImgUrl { get; set; }

        public string UnionId { get; set; }

        public FromClient FromClient { get; set; } = 0;

        public string City { get; set; }
        public bool IsPhoneNumberConfirmed { get; set; }

        public object Extension { get; set; }

        public Dictionary<string, string> UserLogins { get; set; } = [];
    }
}