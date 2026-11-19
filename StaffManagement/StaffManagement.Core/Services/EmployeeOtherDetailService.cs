using StaffManagement.Core.DTOs.EmployeeModel;
using StaffManagement.Core.IRepositories;
using StaffManagement.Core.Iservices;
using StaffManagement.Model.Entities;

namespace StaffManagement.Core.Services;

public sealed class EmployeeOtherDetailService(
    IEmployeeOtherDetailRepository repository,
    IEmployeeRepository employeeRepository,
    IDocumentTypeRepository documentTypeRepository) : IEmployeeOtherDetailService
{
    public async Task<EmployeeOtherDetailDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var detail = await repository.GetByIdAsync(id, cancellationToken);
        return detail is null ? null : new EmployeeOtherDetailDto
        {
            EmployeeOtherDetailId = detail.EmployeeOtherDetailId,
            EmployeeId = detail.EmployeeId,
            DocumentTypeId = detail.DocumentTypeId ?? 0,
            DocumentType = detail.DocumentType,
            DocumentNumber = detail.DocumentNumber,
            ValidFrom = detail.ValidFrom,
            ValidTo = detail.ValidTo,
            OriginalFileName = detail.OriginalFileName,
            StoredFileName = detail.StoredFileName,
            ContentType = detail.ContentType
        };
    }

    public async Task<(bool Succeeded, string? Error)> CreateAsync(EmployeeOtherDetailDto model, CancellationToken cancellationToken = default)
    {
        if (await employeeRepository.GetByIdAsync(model.EmployeeId, cancellationToken) is null)
            return (false, "Employee was not found.");

        var documentType = await documentTypeRepository.GetByIdAsync(model.DocumentTypeId, cancellationToken);
        if (documentType is null || !documentType.IsActive)
            return (false, "Choose an active document type.");

        if (documentType.RequiresValidityDates && (!model.ValidFrom.HasValue || !model.ValidTo.HasValue))
            return (false, "Enter both validity dates for this document type.");
        if (documentType.RequiresValidityDates && model.ValidFrom > model.ValidTo)
            return (false, "Valid To must be on or after Valid From.");

        if (!documentType.RequiresValidityDates)
        {
            model.ValidFrom = null;
            model.ValidTo = null;
        }

        var detail = new EmployeeOtherDetail
        {
            EmployeeId = model.EmployeeId,
            DocumentTypeId = documentType.DocumentTypeId,
            DocumentType = documentType.DocumentTypeName,
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

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var detail = await repository.GetTrackedByIdAsync(id, cancellationToken);
        if (detail is null)
            return false;

        repository.Remove(detail);
        await repository.SaveChangesAsync(cancellationToken);
        return true;
    }
}
