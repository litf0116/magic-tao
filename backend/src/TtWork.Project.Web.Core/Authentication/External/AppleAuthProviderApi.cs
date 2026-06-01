using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.UI;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using TtWork.Project.Web.Models.TokenAuth;

namespace TtWork.Project.Web.Authentication.External;

public class AppleAuthProviderApi : ITransientDependency
{
    private const string AppleKeysUrl = "https://appleid.apple.com/auth/keys";
    private const string AppleIssuer = "https://appleid.apple.com";
    private const string BundleId = "com.molitao.molitaoApp";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AppleAuthProviderApi> _logger;

    public AppleAuthProviderApi(IHttpClientFactory httpClientFactory, ILogger<AppleAuthProviderApi> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<AppleUserInfo> ValidateIdentityTokenAsync(string identityToken)
    {
        if (string.IsNullOrEmpty(identityToken))
        {
            throw new UserFriendlyException("Identity token is required");
        }

        try
        {
            // 1. Get Apple's public keys
            var appleKeys = await GetApplePublicKeysAsync();

            // 2. Decode the JWT header to get the key ID
            var handler = new JwtSecurityTokenHandler();
            var tokenParts = identityToken.Split('.');
            if (tokenParts.Length != 3)
            {
                throw new UserFriendlyException("Invalid identity token format");
            }

            var headerJson = Base64UrlDecode(tokenParts[0]);
            var header = JsonDocument.Parse(headerJson).RootElement;

            if (!header.TryGetProperty("kid", out var kidElement))
            {
                throw new UserFriendlyException("Identity token missing key ID (kid)");
            }

            var keyId = kidElement.GetString();
            if (string.IsNullOrEmpty(keyId))
            {
                throw new UserFriendlyException("Identity token has empty key ID");
            }

            // 3. Find the matching public key from Apple's keys
            var matchingKey = appleKeys.FirstOrDefault(k => k.Kid == keyId);
            if (matchingKey == null)
            {
                throw new UserFriendlyException("Unable to find matching public key for the token");
            }

            // 4. Validate the token with Apple's public key
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = matchingKey.ToRSASecurityKey(),
                ValidateIssuer = true,
                ValidIssuers = new[] { AppleIssuer },
                ValidateAudience = true,
                ValidAudiences = new[] { BundleId },
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(5)
            };

            var principal = handler.ValidateToken(identityToken, validationParameters, out var validatedToken);
            var jwtToken = (JwtSecurityToken)validatedToken;

            // 5. Extract user information from claims
            var subClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "sub");
            var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "email");

            if (subClaim == null || string.IsNullOrEmpty(subClaim.Value))
            {
                throw new UserFriendlyException("Identity token missing subject (sub) claim");
            }

            var appleUserInfo = new AppleUserInfo
            {
                Sub = subClaim.Value,
                Email = emailClaim?.Value,
                // Apple also provides these in initial token but not refresh tokens
                IsPrivateEmail = jwtToken.Claims.FirstOrDefault(c => c.Type == "is_private_email")?.Value == "true",
                RealUserStatus = int.TryParse(jwtToken.Claims.FirstOrDefault(c => c.Type == "real_user_status")?.Value, out var status) ? status : 0
            };

            _logger.LogInformation("[AppleAuth] Token validated successfully for user: {Sub}, Email: {Email}", appleUserInfo.Sub, appleUserInfo.Email ?? "N/A");

            return appleUserInfo;
        }
        catch (SecurityTokenExpiredException)
        {
            _logger.LogWarning("[AppleAuth] Token validation failed: Token has expired");
            throw new UserFriendlyException("Apple identity token has expired");
        }
        catch (SecurityTokenInvalidSignatureException)
        {
            _logger.LogWarning("[AppleAuth] Token validation failed: Invalid signature");
            throw new UserFriendlyException("Apple identity token signature verification failed");
        }
        catch (UserFriendlyException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AppleAuth] Token validation failed with unexpected error");
            throw new UserFriendlyException("Apple identity token validation failed: " + ex.Message);
        }
    }

    private async Task<List<ApplePublicKey>> GetApplePublicKeysAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("AppleAuth");
            var response = await client.GetFromJsonAsync<AppleKeysResponse>(AppleKeysUrl);

            if (response?.Keys == null || response.Keys.Count == 0)
            {
                throw new UserFriendlyException("Failed to retrieve Apple public keys");
            }

            return response.Keys;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AppleAuth] Failed to get Apple public keys");
            throw new UserFriendlyException("Failed to connect to Apple for token verification");
        }
    }

    private static string Base64UrlDecode(string input)
    {
        var output = input.Replace('-', '+').Replace('_', '/');
        switch (output.Length % 4)
        {
            case 2: output += "=="; break;
            case 3: output += "="; break;
        }
        var bytes = Convert.FromBase64String(output);
        return System.Text.Encoding.UTF8.GetString(bytes);
    }
}

public class AppleUserInfo
{
    /// <summary>
    /// Apple user identifier (sub claim)
    /// </summary>
    public string Sub { get; set; }

    /// <summary>
    /// Email address if provided by Apple
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// Whether the email is a private relay email
    /// </summary>
    public bool IsPrivateEmail { get; set; }

    /// <summary>
    /// Real user status (0 = unknown, 1 = likely real, 2 = real user)
    /// </summary>
    public int RealUserStatus { get; set; }
}

internal class AppleKeysResponse
{
    public List<ApplePublicKey> Keys { get; set; }
}

internal class ApplePublicKey
{
    public string Kid { get; set; }
    public string Kty { get; set; }
    public string Alg { get; set; }
    public string Use { get; set; }
    public string N { get; set; }
    public string E { get; set; }

    public RsaSecurityKey ToRSASecurityKey()
    {
        var modulus = Base64UrlDecode(N);
        var exponent = Base64UrlDecode(E);

        var rsaParams = new RSAParameters
        {
            Modulus = modulus,
            Exponent = exponent
        };

        var rsa = RSA.Create();
        rsa.ImportParameters(rsaParams);
        return new RsaSecurityKey(rsa);
    }

    private static byte[] Base64UrlDecode(string input)
    {
        var output = input.Replace('-', '+').Replace('_', '/');
        switch (output.Length % 4)
        {
            case 2: output += "=="; break;
            case 3: output += "="; break;
        }
        return Convert.FromBase64String(output);
    }
}