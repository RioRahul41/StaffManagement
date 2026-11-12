using StaffManagement.Model.Entities;

namespace StaffManagement.Core.Iservices;

public interface IDepartmentService
{
    Task<IReadOnlyList<Department>> GetAllAsync();
    Task<Department?> GetByIdAsync(int departmentId);
    Task<(bool Succeeded, string? Error)> CreateAsync(Department department);
    Task<(bool Succeeded, string? Error)> UpdateAsync(Department department);
    Task<bool> DeleteAsync(int departmentId);
}
