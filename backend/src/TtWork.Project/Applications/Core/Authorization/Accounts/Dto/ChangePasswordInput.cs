using System.ComponentModel.DataAnnotations;

namespace TtWork.Project.Applications.Core.Authorization.Accounts.Dto {
    public class ChangePasswordInput {
        [Required]
        public string CurrentPassword { get; set; }

        [Required]
        public string NewPassword { get; set; }
    }
}
