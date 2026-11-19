using StaffManagement.Core.IRepositories;
using StaffManagement.Core.Iservices;
using StaffManagement.Core.DTOs.EmployeeModel;
using StaffManagement.Core.DTOs.ProfileModel;
using StaffManagement.Model.Entities;
namespace StaffManagement.Core.Services;
public sealed class EmployeeBankDetailService(IEmployeeBankDetailRepository repository, IEmployeeRepository employeeRepository) : IEmployeeBankDetailService
{
    public async Task<BankDetailsDto?> GetByEmployeeIdAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        var detail = await repository.GetByEmployeeIdAsync(employeeId, cancellationToken);
        return detail is null ? null : new BankDetailsDto
        {
            EmployeeBankDetailId = detail.EmployeeBankDetailId,
            EmployeeId = detail.EmployeeId,
            AccountHolderName = detail.AccountHolderName,
            AccountName = detail.AccountName,
            IFSCCode = detail.IFSCCode,
            Branch = detail.Branch,
            AccountNumber = detail.AccountNumber
        };
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
    public async Task<(bool Succeeded, string? Error)> CreateAsync(BankDetailsDto submitted, CancellationToken cancellationToken = default)
    {
        if (await employeeRepository.GetByIdAsync(submitted.EmployeeId, cancellationToken) is null)
            return (false, "Employee was not found.");
        if (await repository.GetByEmployeeIdAsync(submitted.EmployeeId, cancellationToken) is not null)
            return (false, "Bank details already exist for this employee.");

        await repository.AddAsync(new EmployeeBankDetail
        {
            EmployeeId = submitted.EmployeeId,
            AccountHolderName = submitted.AccountHolderName.Trim(),
            AccountName = submitted.AccountName.Trim(),
            IFSCCode = submitted.IFSCCode.Trim().ToUpperInvariant(),
            Branch = submitted.Branch.Trim(),
            AccountNumber = submitted.AccountNumber.Trim()
        }, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    public async Task<(bool Succeeded, string? Error)> UpdateAsync(BankDetailsDto submitted, CancellationToken cancellationToken = default)
    {
        if (await employeeRepository.GetByIdAsync(submitted.EmployeeId, cancellationToken) is null)
            return (false, "Employee was not found.");

        var current = await repository.GetTrackedByEmployeeIdAsync(submitted.EmployeeId, cancellationToken);
        if (current is null || current.EmployeeBankDetailId != submitted.EmployeeBankDetailId)
            return (false, "Bank details were not found.");

        current.AccountHolderName = submitted.AccountHolderName.Trim();
        current.AccountName = submitted.AccountName.Trim();
        current.IFSCCode = submitted.IFSCCode.Trim().ToUpperInvariant();
        current.Branch = submitted.Branch.Trim();
        current.AccountNumber = submitted.AccountNumber.Trim();
        await repository.SaveChangesAsync(cancellationToken);
        return (true, null);
    }
}
