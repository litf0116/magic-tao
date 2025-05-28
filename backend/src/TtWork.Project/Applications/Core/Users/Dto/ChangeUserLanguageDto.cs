using System.ComponentModel.DataAnnotations;

namespace TtWork.Project.Users.Dto {
    public class ChangeUserLanguageDto {
        [Required] public string LanguageName { get; set; }
    }
}