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
        try
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
                var admin = new Admin
                {
                    UserName = "spladmin",
                    Email = "spladmin@spl.com",
                    Name = "SplAdmin"
                };
                await userManager.CreateAsync(admin, "SplAdmin@123");
            }
        }
        catch (Exception ex)
        {
            var logger = sp.GetService<ILogger<DbInitializer>>();
            logger?.LogError(ex, "An error occurred while seeding the database.");
        }
    }
}
