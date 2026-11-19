using StaffManagement.Core.DTOs.DocumentTypeModel;
using StaffManagement.Core.IRepositories;
using StaffManagement.Core.Iservices;
using StaffManagement.Model.Entities;

namespace StaffManagement.Core.Services;

public sealed class DocumentTypeService(IDocumentTypeRepository repository) : IDocumentTypeService
{
    public async Task<IReadOnlyList<DocumentTypeDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
        (await repository.GetAllAsync(cancellationToken)).Select(ToDto).ToList();

    public async Task<IReadOnlyList<DocumentTypeDto>> GetActiveAsync(CancellationToken cancellationToken = default) =>
        (await repository.GetActiveAsync(cancellationToken)).Select(ToDto).ToList();

    public async Task<DocumentTypeDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetByIdAsync(id, cancellationToken);
        return entity is null ? null : ToDto(entity);
    }

    public async Task<(bool Succeeded, string? Error)> CreateAsync(DocumentTypeDto model, CancellationToken cancellationToken = default)
    {
        model.DocumentTypeName = model.DocumentTypeName.Trim();
        if (await repository.ExistsByNameAsync(model.DocumentTypeName, cancellationToken: cancellationToken))
            return (false, "A document type with this name already exists.");

        await repository.AddAsync(new DocumentType
        {
            DocumentTypeName = model.DocumentTypeName,
            RequiresValidityDates = model.RequiresValidityDates,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        }, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    public async Task<(bool Succeeded, string? Error)> UpdateAsync(DocumentTypeDto model, CancellationToken cancellationToken = default)
    {
        model.DocumentTypeName = model.DocumentTypeName.Trim();
        if (await repository.ExistsByNameAsync(model.DocumentTypeName, model.DocumentTypeId, cancellationToken))
            return (false, "A document type with this name already exists.");

        var entity = await repository.GetByIdAsync(model.DocumentTypeId, cancellationToken);
        if (entity is null)
            return (false, "Document type was not found.");

        entity.DocumentTypeName = model.DocumentTypeName;
        entity.RequiresValidityDates = model.RequiresValidityDates;
        entity.IsActive = model.IsActive;
        entity.UpdatedAtUtc = DateTime.UtcNow;
        await repository.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    public async Task<bool> DeactivateAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
            return false;

        entity.IsActive = false;
        entity.UpdatedAtUtc = DateTime.UtcNow;
        await repository.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static DocumentTypeDto ToDto(DocumentType source) => new()
    {
        DocumentTypeId = source.DocumentTypeId,
        DocumentTypeName = source.DocumentTypeName,
        RequiresValidityDates = source.RequiresValidityDates,
        IsActive = source.IsActive,
        CreatedAtUtc = source.CreatedAtUtc,
        UpdatedAtUtc = source.UpdatedAtUtc
    };
}
