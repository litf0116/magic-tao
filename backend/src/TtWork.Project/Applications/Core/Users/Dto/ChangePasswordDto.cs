using System.ComponentModel.DataAnnotations;

namespace TtWork.Project.Users.Dto {
    public class ChangePasswordDto {
        [Required] public string CurrentPassword { get; set; }

        [Required] public string NewPassword { get; set; }
    }
}