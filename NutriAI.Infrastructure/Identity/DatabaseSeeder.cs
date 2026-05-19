using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NutriAI.Application.Configuration;
using NutriAI.Domain.Constants;
using NutriAI.Domain.Entities;
using NutriAI.Infrastructure.Data;

namespace NutriAI.Infrastructure.Identity;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseSeeder");
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var adminSettings = scope.ServiceProvider.GetRequiredService<IOptions<AdminSeedSettings>>().Value;

        await context.Database.MigrateAsync();
        await NutritionFallbackSeeder.SeedAsync(context);

        foreach (var role in new[] { Roles.Admin, Roles.User })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        if (string.IsNullOrWhiteSpace(adminSettings.Email) || string.IsNullOrWhiteSpace(adminSettings.Password))
        {
            logger.LogWarning("AdminSeed email or password not configured; skipping default admin creation.");
            return;
        }

        var admin = await userManager.FindByEmailAsync(adminSettings.Email);
        if (admin == null)
        {
            admin = new ApplicationUser
            {
                UserName = adminSettings.Email,
                Email = adminSettings.Email,
                FullName = adminSettings.FullName,
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(admin, adminSettings.Password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, Roles.Admin);
                logger.LogInformation("Default admin account created: {Email}", adminSettings.Email);
            }
            else
            {
                logger.LogWarning("Failed to create admin: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
        else
        {
            if (!await userManager.IsInRoleAsync(admin, Roles.Admin))
                await userManager.AddToRoleAsync(admin, Roles.Admin);

            if (await userManager.IsInRoleAsync(admin, Roles.User))
                await userManager.RemoveFromRoleAsync(admin, Roles.User);
        }
    }
}
