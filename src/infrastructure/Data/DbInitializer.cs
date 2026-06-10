using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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

        var userManager = sp.GetRequiredService<UserManager<Admin>>();
        var admin = await userManager.FindByEmailAsync("spladmin@spl.com");

        if (admin is null)
        {
            admin = new Admin
            {
                UserName = "spladmin",
                Email = "spladmin@spl.com",
                Name = "SplAdmin"
            };

            await userManager.CreateAsync(admin, "SplAdmin@123");
        }
        else if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("RESET_ADMIN_PASSWORD")))
        {
            admin.PasswordHash = userManager.PasswordHasher.HashPassword(admin, "SplAdmin@123");
            await userManager.UpdateAsync(admin);
        }
    }
}
