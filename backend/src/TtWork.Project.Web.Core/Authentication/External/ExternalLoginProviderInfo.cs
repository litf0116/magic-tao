using System;

namespace TtWork.Project.Authentication.External {
    public class ExternalLoginProviderInfo(string name, string clientId, string clientSecret, Type providerApiType) {
        public string Name { get; set; } = name;

        public string ClientId { get; set; } = clientId;

        public string ClientSecret { get; set; } = clientSecret;

        public Type ProviderApiType { get; set; } = providerApiType;
    }
}