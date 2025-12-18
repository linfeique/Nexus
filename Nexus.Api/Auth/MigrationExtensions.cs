using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Nexus.Api.Auth;

public static class MigrationExtensions
{
    public static async Task ApplyMigrations(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        await using var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        
        await dbContext.Database.MigrateAsync();
        
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        if (!await roleManager.RoleExistsAsync(Roles.Administrator))
        {
            await roleManager.CreateAsync(new IdentityRole(Roles.Administrator));
        }
        
        if (!await roleManager.RoleExistsAsync(Roles.User))
        {
            await roleManager.CreateAsync(new IdentityRole(Roles.User));
        }
        
        if (!await roleManager.RoleExistsAsync(Roles.Deliver))
        {
            await roleManager.CreateAsync(new IdentityRole(Roles.Deliver));
        }
    }
}