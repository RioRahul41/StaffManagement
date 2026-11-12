using StaffManagement.Core.IRepositories;
using StaffManagement.Core.Iservices;
using StaffManagement.Model.Entities;

namespace StaffManagement.Core.Services;

public class DepartmentService(IDepartmentRepository departmentRepository) : IDepartmentService
{
    public Task<IReadOnlyList<Department>> GetAllAsync()
    {
        return departmentRepository.GetAllAsync();
    }

    public Task<Department?> GetByIdAsync(int departmentId)
    {
        return departmentRepository.GetByIdAsync(departmentId);
    }

    public async Task<(bool Succeeded, string? Error)> CreateAsync(Department department)
    {
        department.DepartmentName = department.DepartmentName.Trim();

        if (await departmentRepository.ExistsByNameAsync(department.DepartmentName))
        {
            return (false, "A department with this name already exists.");
        }

        department.CreatedDate = DateTime.Now;
        department.UpdatedDate = null;
        department.IsActive = true;

        await departmentRepository.AddAsync(department);
        return (true, null);
    }

    public async Task<(bool Succeeded, string? Error)> UpdateAsync(Department department)
    {
        department.DepartmentName = department.DepartmentName.Trim();

        if (await departmentRepository.ExistsByNameAsync(
                department.DepartmentName, department.DepartmentId))
        {
            return (false, "A department with this name already exists.");
        }

        department.UpdatedDate = DateTime.Now;
        await departmentRepository.UpdateAsync(department);
        return (true, null);
    }

    public async Task<bool> DeleteAsync(int departmentId)
    {
        var department = await departmentRepository.GetByIdAsync(departmentId);
        if (department is null)
        {
            return false;
        }

        await departmentRepository.DeleteAsync(department);
        return true;
    }
}
