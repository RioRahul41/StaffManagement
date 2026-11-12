using StaffManagement.Model.Entities;

namespace StaffManagement.Core.IRepositories;

public interface IDepartmentRepository
{
    Task<IReadOnlyList<Department>> GetAllAsync();
    Task<Department?> GetByIdAsync(int departmentId);
    Task<bool> ExistsByNameAsync(string departmentName, int? excludedId = null);
    Task AddAsync(Department department);
    Task UpdateAsync(Department department);
    Task DeleteAsync(Department department);
}
