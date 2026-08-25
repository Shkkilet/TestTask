using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Identity
{
    public static class IdentitySeederExtension
    {
        public static async Task SeedIdentityAsync(
            this WebApplication app)
        {
            using var scope = app.Services.CreateScope();

            await IdentitySeeder.SeedRolesAsync(scope.ServiceProvider);
        }
    }
}
