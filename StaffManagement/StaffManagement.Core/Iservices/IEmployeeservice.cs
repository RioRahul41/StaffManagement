using StaffManagement.Core.DTOs.EmployeeModel;

namespace StaffManagement.Core.Iservices
{
    public interface IEmployeeService
    {
        Task<IReadOnlyList<EmployeeDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<EmployeeDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<EmployeeDto?> GetByIdentityUserIdAsync(string identityUserId, CancellationToken cancellationToken = default);
        Task<(bool Succeeded, string? Error)> CreateAsync(EmployeeDto model, string identityUserId, CancellationToken cancellationToken = default);
        Task<(bool Succeeded, string? Error)> UpdateAsync(EmployeeDto model, CancellationToken cancellationToken = default);
        Task<(bool Succeeded, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
