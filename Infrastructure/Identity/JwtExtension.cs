using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Identity
{
    public static class JwtExtension
    {
        public static IServiceCollection AddJWTAuth(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtKey = configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key is not configured.");
            services
           .AddAuthentication(options =>
           {
               options.DefaultAuthenticateScheme =
                   JwtBearerDefaults.AuthenticationScheme;

               options.DefaultChallengeScheme =
                   JwtBearerDefaults.AuthenticationScheme;
           })
           .AddJwtBearer(options =>
           {
               options.TokenValidationParameters =
                   new TokenValidationParameters
                   {
                       ValidateIssuer = true,
                       ValidateAudience = true,
                       ValidateLifetime = true,
                       ValidateIssuerSigningKey = true,

                       ValidIssuer = configuration["Jwt:Issuer"],
                       ValidAudience = configuration["Jwt:Audience"],

                       IssuerSigningKey =
                           new SymmetricSecurityKey(
                               Encoding.UTF8.GetBytes(jwtKey))
                   };
           });

            return services;
        }
    }
}
