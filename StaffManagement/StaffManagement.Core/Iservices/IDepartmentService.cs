using StaffManagement.Core.DTOs.DepartmentModel;
using StaffManagement.Model.Entities;

namespace StaffManagement.Core.Iservices;

public interface IDepartmentService
{
    Task<IReadOnlyList<DepartmentDto>> GetAllAsync();
    Task<IReadOnlyList<DepartmentDto>> GetAllForSelectionAsync();
    Task<DepartmentDto?> GetByIdAsync(int departmentId);
    Task<(bool Succeeded, string? Error)> CreateAsync(DepartmentDto department);
    Task<(bool Succeeded, string? Error)> UpdateAsync(DepartmentDto department);
    Task<bool> DeleteAsync(int departmentId);
}
