using StaffManagement.Core.IRepositories;
using StaffManagement.Core.Iservices;
using StaffManagement.Model.Entities;
namespace StaffManagement.Core.Services;
public sealed class EmployeeExperienceService(IEmployeeExperienceRepository experienceRepository, IEmployeeRepository employeeRepository) : IEmployeeExperienceService
{
    public Task<IReadOnlyList<EmployeeExperience>> GetByEmployeeIdAsync(int employeeId, CancellationToken cancellationToken = default) => experienceRepository.GetByEmployeeIdAsync(employeeId, cancellationToken);
    public Task<EmployeeExperience?> GetByIdAsync(int id, CancellationToken cancellationToken = default) => experienceRepository.GetByIdAsync(id, cancellationToken);
    public Task<Employee?> GetEmployeeByIdAsync(int employeeId, CancellationToken cancellationToken = default) => employeeRepository.GetByIdAsync(employeeId, cancellationToken);
    public async Task<bool> CreateAsync(EmployeeExperience experience, CancellationToken cancellationToken = default)
    {
        if (await employeeRepository.GetByIdAsync(experience.EmployeeId, cancellationToken) is null) return false;
        await experienceRepository.AddAsync(experience, cancellationToken);
        await experienceRepository.SaveChangesAsync(cancellationToken);
        return true;
    }
    public async Task<bool> UpdateAsync(EmployeeExperience submitted, CancellationToken cancellationToken = default)
    {
        var current = await experienceRepository.GetTrackedByIdAsync(submitted.EmployeeExperienceId, cancellationToken);
        if (current is null || current.EmployeeId != submitted.EmployeeId) return false;
        current.PreviousCompany = submitted.PreviousCompany;
        current.DateOfJoining = submitted.DateOfJoining;
        current.DateOfRelieving = submitted.DateOfRelieving;
        await experienceRepository.SaveChangesAsync(cancellationToken);
        return true;
    }
    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var current = await experienceRepository.GetTrackedByIdAsync(id, cancellationToken);
        if (current is null) return false;
        experienceRepository.Remove(current);
        await experienceRepository.SaveChangesAsync(cancellationToken);
        return true;
    }
}
