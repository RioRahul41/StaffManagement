using StaffManagement.Core.DTOs.EmployeeModel;
using StaffManagement.Core.DTOs.ProfileModel;
using StaffManagement.Model.Entities;
namespace StaffManagement.Core.Iservices;
public interface IEmployeeExperienceService
{
    Task<IReadOnlyList<ExperienceInformationDto>> GetByEmployeeIdAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<ExperienceInformationDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<EmployeeDto?> GetEmployeeByIdAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<(bool Succeeded, string? Error)> CreateAsync(ExperienceInformationDto experience, CancellationToken cancellationToken = default);
    Task<(bool Succeeded, string? Error)> UpdateAsync(ExperienceInformationDto experience, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
