using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimplePartyList.Core.Entities;

namespace SimplePartyList.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(SimplePartyListContext context, IServiceProvider sp)
    {
        if (context.Database.IsRelational())
        {
            var pending = await context.Database.GetPendingMigrationsAsync();
            if (pending.Any())
                await context.Database.MigrateAsync();
        }

        if (!await context.Users.AnyAsync(u => u.Email == "spladmin@spl.com"))
        {
            var userManager = sp.GetRequiredService<UserManager<Admin>>();
            var logger = sp.GetRequiredService<ILogger<Admin>>();

            var admin = new Admin
            {
                UserName = "spladmin",
                Email = "spladmin@spl.com",
                Name = "SplAdmin"
            };

            var seedPassword = Environment.GetEnvironmentVariable("SEED_ADMIN_PASSWORD");
            if (string.IsNullOrEmpty(seedPassword))
            {
                seedPassword = Guid.NewGuid().ToString("N") + "Aa1!";
                logger.LogWarning("SEED_ADMIN_PASSWORD not set. Generated random password for spladmin: {Password}", seedPassword);
                logger.LogWarning("SET the SEED_ADMIN_PASSWORD environment variable on the server to use a fixed password.");
            }

            await userManager.CreateAsync(admin, seedPassword);
        }
    }
}
