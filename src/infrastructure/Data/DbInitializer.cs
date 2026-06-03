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
            await context.Database.MigrateAsync();

        var userManager = sp.GetRequiredService<UserManager<Admin>>();
        if (await userManager.FindByEmailAsync("spladmin@spl.com") is null)
        {
            var admin = new Admin
            {
                UserName = "spladmin",
                Email = "spladmin@spl.com",
                Name = "SplAdmin"
            };
            await userManager.CreateAsync(admin, "SplAdmin@123");
        }
    }
}
