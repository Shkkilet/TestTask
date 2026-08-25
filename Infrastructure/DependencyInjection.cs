using Application.Authentication.Interfaces;
using Application.Interfaces;
using Application.Services;
using Infrastructure.Data;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString = ConnectionStringResolver.Resolve(configuration);

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            services.AddDbContext<IdentityDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });

            services.AddJWTAuth(configuration);

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = "Smart";
                options.DefaultAuthenticateScheme = "Smart";
                options.DefaultChallengeScheme = "Smart";
            })
            .AddPolicyScheme("Smart", "Smart Scheme Selector", options =>
            {
                options.ForwardDefaultSelector = context =>
                {
                    var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
                    return authHeader is not null && authHeader.StartsWith("Bearer ")
                        ? JwtBearerDefaults.AuthenticationScheme  
                        : "MvcCookie";                             
                };
            })
            .AddCookie("MvcCookie", options =>
            {
                options.Cookie.Name = "ShortUrl.MvcAuth";
                options.LoginPath = "/api/Auth/login";
                options.AccessDeniedPath = "/About";
            });
            services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
            }).AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<IdentityDbContext>()
            .AddSignInManager().AddDefaultTokenProviders();

            services.AddScoped<IApplicationDbContext, ApplicationDbContext>();

            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<ShortCodeGenerator>();

            return services;
        }
    }
}