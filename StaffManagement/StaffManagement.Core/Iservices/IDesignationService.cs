using StaffManagement.Core.DTOs.DesignationModel;
using StaffManagement.Model.Entities;

namespace StaffManagement.Core.Iservices;

public interface IDesignationService
{
    Task<IReadOnlyList<DesignationDto>> GetAllAsync();
    Task<DesignationDto?> GetByIdAsync(int designationId);
    Task<(bool Succeeded, string? Error)> CreateAsync(DesignationDto designation);
    Task<(bool Succeeded, string? Error)> UpdateAsync(DesignationDto designation);
    Task<bool> DeleteAsync(int designationId);
}
