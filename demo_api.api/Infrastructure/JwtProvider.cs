using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using demo_api.models.Models;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace demo_api.api.Infrastructure;

internal static class JwtProvider
{
    internal static string GenerateAccessToken(ApplicationUser? user, IList<string> roles, JwtSettings config)
    {
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.SecretKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        List<Claim> claims =
        [
            new(ClaimTypes.NameIdentifier, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new(JwtRegisteredClaimNames.Name, user.FirstName!),
            new(JwtRegisteredClaimNames.GivenName, user.LastName!),
            new("CompanyId", user.CompanyId.ToString() ?? string.Empty),
            ..roles.Select(r => new Claim(ClaimTypes.Role, r))
        ];

        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(config.ExpirationInMinutes),
            SigningCredentials = credentials,
            Issuer = config.Issuer,
            Audience = config.Audience
        };

        var tokenHandler = new JsonWebTokenHandler();
        string accessToken = tokenHandler.CreateToken(tokenDescriptor);

        return accessToken;
    }

    internal static string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    }
}