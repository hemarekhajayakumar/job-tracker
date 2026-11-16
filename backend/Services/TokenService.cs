using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using JobTracker.Api.Models;
using Microsoft.IdentityModel.Tokens;

namespace JobTracker.Api.Services;

// Creates the JWT (JSON Web Token) a user receives after logging in.
// The token is like a signed ID card: the browser shows it on every request,
// and the API trusts it because only the API knows the secret key used to sign it.
public class TokenService
{
    public const string Issuer = "JobTracker.Api";

    private readonly string _key;

    public TokenService(IConfiguration configuration)
    {
        _key = configuration["Jwt:Key"]!;
    }

    public string CreateToken(User user)
    {
        // Claims = facts about the user that are written inside the token.
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email)
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: Issuer,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
