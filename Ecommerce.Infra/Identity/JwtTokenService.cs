using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Ecommerce.Application.Interfaces.Infra;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Ecommerce.Infra.Identity;

public class JwtTokenService : ITokenService
{
    private readonly SymmetricSecurityKey _key;
    private readonly string _jwtIssuer;
    private readonly string _jwtAudience;
    private readonly DateTime _expires;

    public JwtTokenService(IConfiguration configuration)
    {
        var jwtIssuer = configuration["Jwt:Issuer"] ?? throw new ArgumentNullException("JWT Issuer is not configured.");
        var jwtAudience = configuration["Jwt:Audience"] ?? throw new ArgumentNullException("JWT Audience is not configured.");
        var jwtKey = configuration["Jwt:Key"] ?? throw new ArgumentNullException("JWT Key is not configured.");
        var expires = configuration["Jwt:ExpiryInMinutes"] ?? throw new ArgumentNullException("JWT ExpiryInMinutes is not configured.");

        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        _jwtIssuer = jwtIssuer;
        _jwtAudience = jwtAudience;
        _expires = DateTime.UtcNow.AddMinutes(double.Parse(expires));
    }

    public string GenerateAccessToken(IUser user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email),
        };

        var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtIssuer,
            audience: _jwtAudience,
            claims: claims,
            expires: _expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }


}