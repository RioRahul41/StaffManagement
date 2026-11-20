using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StaffManagement.Core.DTOs.EmployeeModel;
using StaffManagement.Core.IServices;
using StaffManagement.Core.Iservices;
using StaffManagement.Core.Security;
using StaffManagement.Data.DbContext;
using StaffManagement.Model.Entities;

namespace StaffManagement.Services;

public sealed class IdentityAccountService(
    UserManager<ApplicationIdentity> userManager,
    RoleManager<IdentityRole> roleManager,
    IEmployeeService employeeService,
    ApplicationDbContext dbContext) : IAccountService
{
    public IReadOnlyList<string> GetCreatableRoles(IReadOnlyCollection<string> creatorRoles)
    {
        if (creatorRoles.Contains(RoleNames.Admin, StringComparer.OrdinalIgnoreCase))
            return [RoleNames.Hr, RoleNames.Manager, RoleNames.User, RoleNames.TechnicalSupport];

        if (creatorRoles.Contains(RoleNames.Hr, StringComparer.OrdinalIgnoreCase))
            return [RoleNames.User];

        if (creatorRoles.Contains(RoleNames.Manager, StringComparer.OrdinalIgnoreCase))
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

        var leaveAllowance = RoleLeavePolicy.For(model.Role);
        model.SickLeaveAllowance = leaveAllowance.Sick;
        model.CasualLeaveAllowance = leaveAllowance.Casual;

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var user = new ApplicationIdentity
            {
                UserName = model.Email.Trim(),
                Email = model.Email.Trim(),
                FirstName = model.FirstName.Trim(),
                LastName = model.LastName.Trim(),
                PhoneNumber = string.IsNullOrWhiteSpace(model.Phone) ? null : model.Phone.Trim(),
                Location = model.Location.Trim(),
                CreatedTime = DateTime.UtcNow,
                UserIsActive = true
            };

            var createResult = await userManager.CreateAsync(user, model.Password);
            if (!createResult.Succeeded)
                return new(false, FormatErrors(createResult));

            var roleResult = await userManager.AddToRoleAsync(user, model.Role);
            if (!roleResult.Succeeded)
                return new(false, FormatErrors(roleResult));

            var employeeResult = await employeeService.CreateAsync(
                model,
                user.Id,
                cancellationToken);

            if (!employeeResult.Succeeded)
                return new(false, employeeResult.Error);

            await transaction.CommitAsync(cancellationToken);
            return new(true, null, user.Id);
        }
        catch (DbUpdateException exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            return new(false, exception.InnerException?.Message ?? exception.Message);
        }
        catch (Exception exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            return new(false, exception.Message);
        }
    }

    private static string FormatErrors(IdentityResult result) =>
        string.Join(" ", result.Errors.Select(error => error.Description));
}
