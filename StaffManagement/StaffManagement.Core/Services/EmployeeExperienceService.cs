using StaffManagement.Core.IRepositories;
using StaffManagement.Core.Iservices;
using StaffManagement.Core.DTOs.EmployeeModel;
using StaffManagement.Core.DTOs.ProfileModel;
using StaffManagement.Model.Entities;
namespace StaffManagement.Core.Services;
public sealed class EmployeeExperienceService(IEmployeeExperienceRepository experienceRepository, IEmployeeRepository employeeRepository) : IEmployeeExperienceService
{
    public async Task<IReadOnlyList<ExperienceInformationDto>> GetByEmployeeIdAsync(int employeeId, CancellationToken cancellationToken = default) =>
        (await experienceRepository.GetByEmployeeIdAsync(employeeId, cancellationToken)).Select(ToDto).ToList();

    public async Task<ExperienceInformationDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var experience = await experienceRepository.GetByIdAsync(id, cancellationToken);
        return experience is null ? null : ToDto(experience);
    }

    public async Task<EmployeeDto?> GetEmployeeByIdAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        var employee = await employeeRepository.GetByIdAsync(employeeId, cancellationToken);
        return employee is null ? null : new EmployeeDto
        {
            Id = employee.Id,
            FirstName = employee.FirstName,
            LastName = employee.LastName
        };
    }

    public async Task<(bool Succeeded, string? Error)> CreateAsync(ExperienceInformationDto experience, CancellationToken cancellationToken = default)
    {
        if (await employeeRepository.GetByIdAsync(experience.EmployeeId, cancellationToken) is null)
            return (false, "Employee was not found.");

        await experienceRepository.AddAsync(ToEntity(experience), cancellationToken);
        await experienceRepository.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    public async Task<(bool Succeeded, string? Error)> UpdateAsync(ExperienceInformationDto submitted, CancellationToken cancellationToken = default)
    {
        var current = await experienceRepository.GetTrackedByIdAsync(submitted.EmployeeExperienceId, cancellationToken);
        if (current is null || current.EmployeeId != submitted.EmployeeId)
            return (false, "Experience information was not found.");

        current.PreviousCompany = submitted.PreviousCompany;
        current.DateOfJoining = submitted.DateOfJoining;
        current.DateOfRelieving = submitted.DateOfRelieving;
        await experienceRepository.SaveChangesAsync(cancellationToken);
        return (true, null);
    }
    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var current = await experienceRepository.GetTrackedByIdAsync(id, cancellationToken);
        if (current is null) return false;
        experienceRepository.Remove(current);
        await experienceRepository.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static EmployeeExperience ToEntity(ExperienceInformationDto source) => new()
    {
        EmployeeExperienceId = source.EmployeeExperienceId,
        EmployeeId = source.EmployeeId,
        PreviousCompany = source.PreviousCompany.Trim(),
        DateOfJoining = source.DateOfJoining,
        DateOfRelieving = source.DateOfRelieving
    };

    private static ExperienceInformationDto ToDto(EmployeeExperience source) => new()
    {
        EmployeeExperienceId = source.EmployeeExperienceId,
        EmployeeId = source.EmployeeId,
        PreviousCompany = source.PreviousCompany,
        DateOfJoining = source.DateOfJoining,
        DateOfRelieving = source.DateOfRelieving
    };
}
