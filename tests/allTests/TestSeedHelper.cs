using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SimplePartyList.Core.Entities;

namespace SimplePartyList.Tests;

public static class TestSeedHelper
{
    public static async Task SeedAdminAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Admin>>();
        if (await userManager.FindByEmailAsync("spladmin@spl.com") is null)
        {
            var admin = new Admin
            {
                UserName = "spladmin",
                Email = "spladmin@spl.com",
                Name = "SplAdmin"
            };
            var result = await userManager.CreateAsync(admin, "SplAdmin@123");
            if (!result.Succeeded)
                throw new InvalidOperationException(
                    "Falha ao criar admin: " + string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }
}
