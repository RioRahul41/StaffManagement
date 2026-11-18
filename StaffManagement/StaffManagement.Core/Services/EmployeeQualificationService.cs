using StaffManagement.Core.IRepositories;
using StaffManagement.Core.Iservices;
using StaffManagement.Model.Entities;

namespace StaffManagement.Core.Services;

public sealed class EmployeeQualificationService(
    IEmployeeQualificationRepository qualificationRepository,
    IEmployeeRepository employeeRepository) : IEmployeeQualificationService
{
    public Task<IReadOnlyList<EmployeeQualification>> GetByEmployeeIdAsync(int employeeId, CancellationToken cancellationToken = default) =>
        qualificationRepository.GetByEmployeeIdAsync(employeeId, cancellationToken);

    public Task<EmployeeQualification?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        qualificationRepository.GetByIdAsync(id, cancellationToken);

    public Task<Employee?> GetEmployeeByIdAsync(int employeeId, CancellationToken cancellationToken = default) =>
        employeeRepository.GetByIdAsync(employeeId, cancellationToken);

    public async Task<(bool Succeeded, string? Error)> CreateAsync(EmployeeQualification qualification, CancellationToken cancellationToken = default)
    {
        if (await employeeRepository.GetByIdAsync(qualification.EmployeeId, cancellationToken) is null)
            return (false, "Employee was not found.");

        await qualificationRepository.AddAsync(qualification, cancellationToken);
        await qualificationRepository.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    public async Task<bool> UpdateAsync(EmployeeQualification submitted, CancellationToken cancellationToken = default)
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
    }}
