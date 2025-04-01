using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using RssFeedReader.Interfaces;
using RssFeedReader.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RssFeedReader.Services;

public class TokenService(UserManager<AppUser> userManager) : ITokenService
{
    public string CreateToken(AppUser user)
    {
        var tokenKey = Environment.GetEnvironmentVariable("TOKEN_KEY")
            ?? throw new ArgumentNullException(
                "TokenKey is missing in environment variables");

        if (tokenKey.Length < 64)
        {
            throw new ArgumentNullException("Your TokenKey needs to be longer.");
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey));

        if (user.UserName is null)
        {
            throw new ArgumentNullException(nameof(user.UserName));
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName)
        };

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(2),
            SigningCredentials = creds
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}
