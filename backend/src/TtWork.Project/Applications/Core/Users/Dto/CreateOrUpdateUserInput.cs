using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TtWork.Project.Applications.Users.Dto;

namespace TtWork.Project.Applications.Core.Users.Dto {
    public class CreateOrUpdateUserInput {
        [Required] public UserEditDto User { get; set; }

        [Required] public string[] AssignedRoleNames { get; set; }

        public List<long> OrganizationUnits { get; set; }

        public bool SetRandomPassword { get; set; }

        public CreateOrUpdateUserInput() {
            OrganizationUnits = new List<long>();
        }
    }
}