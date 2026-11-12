using StaffManagement.Core.IRepositories;
using StaffManagement.Core.Iservices;
using StaffManagement.Model.Entities;

namespace StaffManagement.Core.Services;

public sealed class DesignationService(IDesignationRepository designationRepository) : IDesignationService
{
    public Task<IReadOnlyList<Designation>> GetAllAsync()
    {
        return designationRepository.GetAllAsync();
    }

    public Task<Designation?> GetByIdAsync(int designationId)
    {
        return designationRepository.GetByIdAsync(designationId);
    }

    public async Task<(bool Succeeded, string? Error)> CreateAsync(Designation designation)
    {
        designation.DesignationName = designation.DesignationName.Trim();

        if (await designationRepository.ExistsByNameAsync(designation.DesignationName))
        {
            return (false, "A designation with this name already exists.");
        }

        designation.CreatedDate = DateTime.UtcNow;
        designation.UpdatedDate = null;
        designation.IsActive = true;

        await designationRepository.AddAsync(designation);
        return (true, null);
    }

    public async Task<(bool Succeeded, string? Error)> UpdateAsync(Designation designation)
    {
        designation.DesignationName = designation.DesignationName.Trim();

        if (await designationRepository.ExistsByNameAsync(
                designation.DesignationName,
                designation.DesignationId))
        {
            return (false, "A designation with this name already exists.");
        }

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
}
