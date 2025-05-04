using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace HomestayBookingAPI.Configuration
{
    public static class AuthenticationConfig
    {
        public static IServiceCollection AddAuthenticationConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            var secretKey = Encoding.UTF8.GetBytes(configuration["JwtSettings:Secret"]);

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = configuration["JwtSettings:Issuer"],

                        ValidateAudience = true,
                        ValidAudience = configuration["JwtSettings:Audience"],

                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(secretKey),

                        ValidateLifetime = true,

                        RoleClaimType = ClaimTypes.Role
                    };
                });

            services.AddAuthorization();

            return services;
        }
    }
}