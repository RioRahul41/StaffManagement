using StaffManagement.Core.DTOs.EmployeeModel;

namespace StaffManagement.Core.IServices;

public sealed record AccountCreationResult(bool Succeeded, string? Error, string? UserId = null);

public interface IAccountService
{
    IReadOnlyList<string> GetCreatableRoles(IReadOnlyCollection<string> creatorRoles);
    Task<AccountCreationResult> CreateAsync(
        CreateAccountDto model,
        IReadOnlyCollection<string> creatorRoles,
        CancellationToken cancellationToken = default);
}
