using System.Collections.Generic;
using TtWork.Project.Applications.Core.Users.Dto;
using TtWork.Project.Users.Dto;

namespace TtWork.Project.Applications.Users.Dto {
    public class GetUserForEditOutput {
        public string HeadImgUrl { get; set; }

        public UserEditDto User { get; set; }

        public UserRoleDto[] Roles { get; set; }

        public List<string> MemberedOrganizationUnits { get; set; }
    }
}