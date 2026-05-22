using System.ComponentModel.DataAnnotations;

namespace TtWork.Project.Applications.Core.Authorization.Accounts.Dto;

public class SendSmsCodeInput
{
    [StringLength(20)]
    [Required]
    public string PhoneNumber { get; set; }

    public string Purpose { get; set; } = "Login";
}