using StaffManagement.Model.Entities;

namespace StaffManagement.Core.Iservices;

public interface IDesignationService
{
    Task<IReadOnlyList<Designation>> GetAllAsync();
    Task<Designation?> GetByIdAsync(int designationId);
    Task<(bool Succeeded, string? Error)> CreateAsync(Designation designation);
    Task<(bool Succeeded, string? Error)> UpdateAsync(Designation designation);
    Task<bool> DeleteAsync(int designationId);
}
