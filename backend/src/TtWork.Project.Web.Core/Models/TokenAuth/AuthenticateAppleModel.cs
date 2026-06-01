namespace TtWork.Project.Web.Models.TokenAuth;

public class AuthenticateAppleModel
{
    /// <summary>
    /// Apple identity_token (JWT)
    /// </summary>
    public string IdentityToken { get; set; }

    /// <summary>
    /// Apple user identifier (sub) - may already be extracted by client
    /// </summary>
    public string UserIdentifier { get; set; }

    /// <summary>
    /// Email address if provided by Apple
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// First name provided by Apple
    /// </summary>
    public string GivenName { get; set; }

    /// <summary>
    /// Last name provided by Apple
    /// </summary>
    public string FamilyName { get; set; }
}