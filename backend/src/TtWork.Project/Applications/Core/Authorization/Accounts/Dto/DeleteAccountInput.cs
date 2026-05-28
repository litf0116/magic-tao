using System.ComponentModel.DataAnnotations;

namespace TtWork.Project.Applications.Core.Authorization.Accounts.Dto;

public class DeleteAccountInput
{
    /// <summary>
    /// 当前密码，用于身份确认
    /// </summary>
    [Required]
    public string Password { get; set; }
}
