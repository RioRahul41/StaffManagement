using StaffManagement.Core.IRepositories;
using StaffManagement.Core.Iservices;
using StaffManagement.Core.DTOs.DesignationModel;
using StaffManagement.Model.Entities;

namespace StaffManagement.Core.Services;

public sealed class DesignationService(IDesignationRepository designationRepository) : IDesignationService
{
    public async Task<IReadOnlyList<DesignationDto>> GetAllAsync() =>
        (await designationRepository.GetAllAsync()).Select(ToDto).ToList();

    public async Task<DesignationDto?> GetByIdAsync(int designationId)
    {
        var designation = await designationRepository.GetByIdAsync(designationId);
        return designation is null ? null : ToDto(designation);
    }

    public async Task<(bool Succeeded, string? Error)> CreateAsync(DesignationDto model)
    {
        model.DesignationName = model.DesignationName.Trim();

        if (await designationRepository.ExistsByNameAsync(model.DesignationName))
        {
            return (false, "A designation with this name already exists.");
        }

        var designation = new Designation
        {
            DesignationName = model.DesignationName,
            DepartmentId = model.DepartmentId,
            Description = model.Description,
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = null,
            IsActive = true
        };

        await designationRepository.AddAsync(designation);
        return (true, null);
    }

    public async Task<(bool Succeeded, string? Error)> UpdateAsync(DesignationDto model)
    {
        model.DesignationName = model.DesignationName.Trim();

        if (await designationRepository.ExistsByNameAsync(
                model.DesignationName,
                model.DesignationId))
        {
            return (false, "A designation with this name already exists.");
        }

        var designation = await designationRepository.GetByIdAsync(model.DesignationId);
        if (designation is null)
            return (false, "Designation was not found.");

        designation.DesignationName = model.DesignationName;
        designation.Description = model.Description;
        designation.DepartmentId = model.DepartmentId;
        designation.IsActive = model.IsActive;
        designation.UpdatedDate = DateTime.UtcNow;
        await designationRepository.UpdateAsync(designation);
        return (true, null);
    }

    public async Task<bool> DeleteAsync(int designationId)
    {
        var designation = await designationRepository.GetByIdAsync(designationId);
        if (designation is null)
        {
            return false;
        }

        await designationRepository.DeleteAsync(designation);
        return true;
    }

    private static DesignationDto ToDto(Designation designation) => new()
    {
        DesignationId = designation.DesignationId,
        DesignationName = designation.DesignationName,
        DepartmentId = designation.DepartmentId,
        DepartmentName = designation.Department?.DepartmentName,
        Description = designation.Description,
        CreatedDate = designation.CreatedDate,
        UpdatedDate = designation.UpdatedDate,
        IsActive = designation.IsActive
    };
}
