using StaffManagement.Core.DTOs.EmployeeModel;
using StaffManagement.Core.IRepositories;
using StaffManagement.Core.Iservices;
using StaffManagement.Model.Entities;

namespace StaffManagement.Core.Services;

public sealed class EmployeeOtherDetailService(
    IEmployeeOtherDetailRepository repository,
    IEmployeeRepository employeeRepository) : IEmployeeOtherDetailService
{
    public async Task<(bool Succeeded, string? Error)> CreateAsync(EmployeeOtherDetailDto model, CancellationToken cancellationToken = default)
    {
        if (await employeeRepository.GetByIdAsync(model.EmployeeId, cancellationToken) is null)
            return (false, "Employee was not found.");

        var detail = new EmployeeOtherDetail
        {
            EmployeeId = model.EmployeeId,
            DocumentType = model.DocumentType.Trim(),
            DocumentNumber = model.DocumentNumber.Trim(),
            ValidFrom = model.ValidFrom,
            ValidTo = model.ValidTo,
            OriginalFileName = model.OriginalFileName,
            StoredFileName = model.StoredFileName,
            ContentType = model.ContentType
        };

        await repository.AddAsync(detail, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return (true, null);
    }
}
