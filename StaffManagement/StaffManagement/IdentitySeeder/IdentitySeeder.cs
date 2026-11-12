using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StaffManagement.Core.Security;
using StaffManagement.Data.DbContext;
using StaffManagement.Model.Entities;

namespace StaffManagement.IdentitySeeder;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationIdentity>>();
        var dbContext = services.GetRequiredService<ApplicationDbContext>();

        await dbContext.Database.MigrateAsync();

        foreach (var role in RoleNames.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(role));
                EnsureSuccess(result, $"creating role '{role}'");
            }
        }

        var admins = await userManager.GetUsersInRoleAsync(RoleNames.Admin);
        if (admins.Count > 1)
            throw new InvalidOperationException("The database contains more than one Admin account.");

        if (admins.Count == 1)
            return;

        var email = configuration["Identity:AdminEmail"];
        var password = configuration["Identity:AdminPassword"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException("Identity:AdminEmail and Identity:AdminPassword must be configured before seeding the Admin account.");

        var admin = new ApplicationIdentity
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FirstName = "System",
            LastName = "Administrator",
            Location = "Head Office",
            CreatedTime = DateTime.UtcNow,
            UserIsActive = true
        };

        var createResult = await userManager.CreateAsync(admin, password);
        EnsureSuccess(createResult, "creating the Admin account");

        var roleResult = await userManager.AddToRoleAsync(admin, RoleNames.Admin);
        EnsureSuccess(roleResult, "assigning the Admin role");

        dbContext.Employees.Add(new Employee
        {
            FirstName = admin.FirstName,
            LastName = admin.LastName,
            Email = email,
            Location = admin.Location,
            CreatedTime = DateTime.UtcNow,
            EmpIsActive = true,
            IdentityUserId = admin.Id
        });
        await dbContext.SaveChangesAsync();
    }

    private static void EnsureSuccess(IdentityResult result, string operation)
    {
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(error => error.Description));
            throw new InvalidOperationException($"Failed while {operation}: {errors}");
        }
    }
}
