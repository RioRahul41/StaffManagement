using StaffManagement.Core.IRepositories;
using StaffManagement.Core.Iservices;
using StaffManagement.Core.DTOs.EmployeeModel;
using StaffManagement.Model.Entities;

namespace StaffManagement.Core.Services;

public sealed class EmployeeQualificationService(
    IEmployeeQualificationRepository qualificationRepository,
    IEmployeeRepository employeeRepository) : IEmployeeQualificationService
{
    public async Task<IReadOnlyList<EmployeeQualificationDto>> GetByEmployeeIdAsync(int employeeId, CancellationToken cancellationToken = default) =>
        (await qualificationRepository.GetByEmployeeIdAsync(employeeId, cancellationToken)).Select(ToDto).ToList();

    public async Task<EmployeeQualificationDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var qualification = await qualificationRepository.GetByIdAsync(id, cancellationToken);
        return qualification is null ? null : ToDto(qualification);
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

    public async Task<(bool Succeeded, string? Error)> CreateAsync(EmployeeQualificationDto model, CancellationToken cancellationToken = default)
    {
        if (await employeeRepository.GetByIdAsync(model.EmployeeId, cancellationToken) is null)
            return (false, "Employee was not found.");

        await qualificationRepository.AddAsync(ToEntity(model), cancellationToken);
        await qualificationRepository.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    public async Task<bool> UpdateAsync(EmployeeQualificationDto submitted, CancellationToken cancellationToken = default)
    {
        var existing = await qualificationRepository.GetTrackedByIdAsync(submitted.EmployeeQualificationId, cancellationToken);
        if (existing is null || existing.EmployeeId != submitted.EmployeeId)
            return false;

        existing.Qualification = submitted.Qualification;
        existing.InstitutionName = submitted.InstitutionName;
        existing.Percentage = submitted.Percentage;
        existing.YearOfPassing = submitted.YearOfPassing;
        existing.OriginalFileName = submitted.OriginalFileName;
        existing.StoredFileName = submitted.StoredFileName;
        existing.ContentType = submitted.ContentType;
        await qualificationRepository.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static EmployeeQualification ToEntity(EmployeeQualificationDto source) => new()
    {
        EmployeeQualificationId = source.EmployeeQualificationId,
        EmployeeId = source.EmployeeId,
        Qualification = source.Qualification.Trim(),
        InstitutionName = source.InstitutionName.Trim(),
        Percentage = source.Percentage,
        YearOfPassing = source.YearOfPassing,
        OriginalFileName = source.OriginalFileName,
        StoredFileName = source.StoredFileName,
        ContentType = source.ContentType
    };

    private static EmployeeQualificationDto ToDto(EmployeeQualification source) => new()
    {
        EmployeeQualificationId = source.EmployeeQualificationId,
        EmployeeId = source.EmployeeId,
        Qualification = source.Qualification,
        InstitutionName = source.InstitutionName,
        Percentage = source.Percentage,
        YearOfPassing = source.YearOfPassing,
        OriginalFileName = source.OriginalFileName,
        StoredFileName = source.StoredFileName,
        ContentType = source.ContentType
    };
}
