using System.Collections.Generic;
using TtWork.Project.Applications.Sessions.Dto;


// ReSharper disable once CheckNamespace
namespace TtWork.Project.Applications.Dto {
    public class GetCurrentLoginInformationsOutput {
        public ApplicationInfoDto Application { get; set; }
        public UserLoginInfoDto User { get; set; }
        public TenantLoginInfoDto Tenant { get; set; }

        public List<string> Permissions { get; set; } = new List<string>();
        public List<string> Roles { get; set; } = new List<string>();
    }
}