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

        await SynchronizeLeaveAllowancesAsync(userManager, dbContext);

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
            IdentityUserId = admin.Id,
            SickLeaveAllowance = RoleLeavePolicy.For(RoleNames.Admin).Sick,
            CasualLeaveAllowance = RoleLeavePolicy.For(RoleNames.Admin).Casual
        });
        await dbContext.SaveChangesAsync();
    }

    private static async Task SynchronizeLeaveAllowancesAsync(
        UserManager<ApplicationIdentity> userManager,
        ApplicationDbContext dbContext)
    {
        var employeesByUserId = await dbContext.Employees
            .Where(employee => !employee.EmpIsDeleted && employee.IdentityUserId != null)
            .ToDictionaryAsync(employee => employee.IdentityUserId!, StringComparer.Ordinal);
        var updatedUserIds = new HashSet<string>(StringComparer.Ordinal);
        var hasChanges = false;

        foreach (var role in RoleNames.All)
        {
            var allowance = RoleLeavePolicy.For(role);
            var users = await userManager.GetUsersInRoleAsync(role);
            foreach (var user in users)
            {
                if (!updatedUserIds.Add(user.Id) || !employeesByUserId.TryGetValue(user.Id, out var employee))
                    continue;

                if (employee.SickLeaveAllowance == allowance.Sick && employee.CasualLeaveAllowance == allowance.Casual)
                    continue;

                employee.SickLeaveAllowance = allowance.Sick;
                employee.CasualLeaveAllowance = allowance.Casual;
                employee.UpdatedTime = DateTime.UtcNow;
                hasChanges = true;
            }
        }

        if (hasChanges)
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
