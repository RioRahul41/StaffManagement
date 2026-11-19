using StaffManagement.Core.IRepositories;
using StaffManagement.Core.Iservices;
using StaffManagement.Core.DTOs.DepartmentModel;
using StaffManagement.Model.Entities;

namespace StaffManagement.Core.Services;

public class DepartmentService(IDepartmentRepository departmentRepository) : IDepartmentService
{
    public async Task<IReadOnlyList<DepartmentDto>> GetAllAsync() =>
        (await departmentRepository.GetAllAsync()).Select(ToDto).ToList();

    public async Task<IReadOnlyList<DepartmentDto>> GetAllForSelectionAsync() =>
        (await departmentRepository.GetAllForSelectionAsync()).Select(ToDto).ToList();

    public async Task<DepartmentDto?> GetByIdAsync(int departmentId)
    {
        var department = await departmentRepository.GetByIdAsync(departmentId);
        return department is null ? null : ToDto(department);
    }

    public async Task<(bool Succeeded, string? Error)> CreateAsync(DepartmentDto model)
    {
        model.DepartmentName = model.DepartmentName.Trim();

        if (await departmentRepository.ExistsByNameAsync(model.DepartmentName))
        {
            return (false, "A department with this name already exists.");
        }

        var department = new Department
        {
            DepartmentName = model.DepartmentName,
            Description = model.Description,
            CreatedDate = DateTime.Now,
            UpdatedDate = null,
            IsActive = true
        };

        await departmentRepository.AddAsync(department);
        return (true, null);
    }

    public async Task<(bool Succeeded, string? Error)> UpdateAsync(DepartmentDto model)
    {
        model.DepartmentName = model.DepartmentName.Trim();

        if (await departmentRepository.ExistsByNameAsync(
                model.DepartmentName, model.DepartmentId))
        {
            return (false, "A department with this name already exists.");
        }

        var department = await departmentRepository.GetByIdAsync(model.DepartmentId);
        if (department is null)
            return (false, "Department was not found.");

        department.DepartmentName = model.DepartmentName;
        department.Description = model.Description;
        department.IsActive = model.IsActive;
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

    private static DepartmentDto ToDto(Department department) => new()
    {
        DepartmentId = department.DepartmentId,
        DepartmentName = department.DepartmentName,
        Description = department.Description,
        CreatedDate = department.CreatedDate,
        UpdatedDate = department.UpdatedDate,
        IsActive = department.IsActive
    };
}
