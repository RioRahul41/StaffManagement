using StaffManagement.Model.Entities;

namespace StaffManagement.Core.IRepositories;

public interface IDesignationRepository
{
    Task<IReadOnlyList<Designation>> GetAllAsync();
    Task<Designation?> GetByIdAsync(int designationId);
    Task<bool> ExistsByNameAsync(string designationName, int? excludedId = null);
    Task AddAsync(Designation designation);
    Task UpdateAsync(Designation designation);
    Task DeleteAsync(Designation designation);
}
