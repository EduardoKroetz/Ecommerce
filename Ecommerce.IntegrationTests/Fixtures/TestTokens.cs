using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Ecommerce.Infra.Identity;
using Microsoft.IdentityModel.Tokens;

namespace Ecommerce.IntegrationTests.Fixtures;

// Builds tokens the API must reject: by default valid for the user, so a test changes only what it checks
public static class TestTokens
{
    public static string Create(TestUser user, DateTime? expires = null, string key = TestJwt.Key, string issuer = TestJwt.Issuer)
    {
        // Real clock: the JWT handler validates the lifetime against it, not against the test clock
        var expiresAt = expires ?? DateTime.UtcNow.AddMinutes(TestJwt.ExpiryInMinutes);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: TestJwt.Audience,
            claims:
            [
                new Claim(ClaimTypes.NameIdentifier, user.UserId),
                new Claim(AppClaimTypes.CustomerId, user.CustomerId.ToString())
            ],
            notBefore: expiresAt.AddHours(-2),
            expires: expiresAt,
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
