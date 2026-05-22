using System.ComponentModel.DataAnnotations;

namespace TtWork.Project.Applications.Core.Authorization.Accounts.Dto;

public class PhoneAuthenticateInput
{
    [StringLength(20)]
    [Required]
    public string PhoneNumber { get; set; }

    [StringLength(6)]
    [Required]
    public string Code { get; set; }
}