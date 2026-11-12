using Microsoft.AspNetCore.Identity;
using StaffManagement.Core.DTOs.EmployeeModel;
using StaffManagement.Core.IServices;
using StaffManagement.Core.Iservices;
using StaffManagement.Core.Security;
using StaffManagement.Model.Entities;

namespace StaffManagement.Services;

public sealed class IdentityAccountService(
    UserManager<ApplicationIdentity> userManager,
    RoleManager<IdentityRole> roleManager,
    IEmployeeService employeeService) : IAccountService
{
    public IReadOnlyList<string> GetCreatableRoles(IReadOnlyCollection<string> creatorRoles)
    {
        if (creatorRoles.Contains(RoleNames.Admin, StringComparer.OrdinalIgnoreCase))
            return [RoleNames.Hr, RoleNames.Manager, RoleNames.User];

        if (creatorRoles.Contains(RoleNames.Hr, StringComparer.OrdinalIgnoreCase))
            return [RoleNames.User];

        return [];
    }

    public async Task<AccountCreationResult> CreateAsync(
        CreateAccountDto model,
        IReadOnlyCollection<string> creatorRoles,
        CancellationToken cancellationToken = default)
    {
        var creatableRoles = GetCreatableRoles(creatorRoles);
        if (!creatableRoles.Contains(model.Role, StringComparer.OrdinalIgnoreCase))
            return new(false, "You are not allowed to create an account with this role.");

        if (!await roleManager.RoleExistsAsync(model.Role))
            return new(false, "The selected role does not exist.");

        var user = new ApplicationIdentity
        {
            UserName = model.Email,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
            PhoneNumber = model.Phone,
            Location = model.Location,
            CreatedTime = DateTime.UtcNow,
            UserIsActive = true
        };

        var createResult = await userManager.CreateAsync(user, model.Password);
        if (!createResult.Succeeded)
            return new(false, FormatErrors(createResult));

        var roleResult = await userManager.AddToRoleAsync(user, model.Role);
        if (!roleResult.Succeeded)
        {
            await userManager.DeleteAsync(user);
            return new(false, FormatErrors(roleResult));
        }

        var employeeResult = await employeeService.CreateAsync(model, user.Id, cancellationToken);
        if (!employeeResult.Succeeded)
        {
            await userManager.DeleteAsync(user);
            return new(false, employeeResult.Error);
        }

        return new(true, null, user.Id);
    }

    private static string FormatErrors(IdentityResult result) =>
        string.Join(" ", result.Errors.Select(error => error.Description));
}
