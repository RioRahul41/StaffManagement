using StaffManagement.Core.IRepositories;
using StaffManagement.Core.Iservices;
using StaffManagement.Model.Entities;
namespace StaffManagement.Core.Services;
public sealed class EmployeeBankDetailService(IEmployeeBankDetailRepository repository, IEmployeeRepository employeeRepository) : IEmployeeBankDetailService
{
    public Task<EmployeeBankDetail?> GetByEmployeeIdAsync(int employeeId, CancellationToken cancellationToken = default) => repository.GetByEmployeeIdAsync(employeeId, cancellationToken);
    public Task<Employee?> GetEmployeeByIdAsync(int employeeId, CancellationToken cancellationToken = default) => employeeRepository.GetByIdAsync(employeeId, cancellationToken);
    public async Task<bool> SaveAsync(EmployeeBankDetail submitted, CancellationToken cancellationToken = default)
    {
        if (await employeeRepository.GetByIdAsync(submitted.EmployeeId, cancellationToken) is null) return false;
        var current = await repository.GetTrackedByEmployeeIdAsync(submitted.EmployeeId, cancellationToken);
        if (current is null) await repository.AddAsync(submitted, cancellationToken);
        else
        {
            current.AccountHolderName = submitted.AccountHolderName;
            current.AccountName = submitted.AccountName;
            current.IFSCCode = submitted.IFSCCode;
            current.Branch = submitted.Branch;
            current.AccountNumber = submitted.AccountNumber;
        }
        await repository.SaveChangesAsync(cancellationToken);
        return true;
    }
}
