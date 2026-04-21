using System.ComponentModel.DataAnnotations;

namespace TtWork.Project.Applications.Core.Authorization.Accounts.Dto;

public class BindPhoneWithPasswordInput
{
    [StringLength(20)]
    [Required]
    [Phone]
    public string PhoneNumber { get; set; }

    [StringLength(128)]
    [Required]
    [MinLength(8)]
    public string Password { get; set; }

    public string BindToken { get; set; }
}

public class BindPhoneResult
{
    public string AccessToken { get; set; }
    public string EncryptedAccessToken { get; set; }
    public int ExpireInSeconds { get; set; }
    public string RefreshToken { get; set; }
    public int RefreshTokenExpireInSeconds { get; set; }
    public long UserId { get; set; }
    public string UserName { get; set; }
}