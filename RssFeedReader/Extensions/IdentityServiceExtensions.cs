using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using RssFeedReader.Data;
using RssFeedReader.Models;
using System.Text;

namespace RssFeedReader.Extensions;

public static class IdentityServiceExtensions
{
    public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddIdentityCore<AppUser>(opt =>
        {
            opt.Password.RequireNonAlphanumeric = false;
        })
        .AddEntityFrameworkStores<FeedContext>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            var tokenKey = Environment.GetEnvironmentVariable("TOKEN_KEY")
                           ?? throw new ArgumentNullException("TOKEN_KEY environment variable is missing");
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey)),
                ValidateIssuer = false,
                ValidateAudience = false
            };
        });

        return services;
    }
}
