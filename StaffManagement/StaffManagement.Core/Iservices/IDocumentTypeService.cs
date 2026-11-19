using StaffManagement.Core.DTOs.DocumentTypeModel;

namespace StaffManagement.Core.Iservices;

public interface IDocumentTypeService
{
    Task<IReadOnlyList<DocumentTypeDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DocumentTypeDto>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<DocumentTypeDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<(bool Succeeded, string? Error)> CreateAsync(DocumentTypeDto documentType, CancellationToken cancellationToken = default);
    Task<(bool Succeeded, string? Error)> UpdateAsync(DocumentTypeDto documentType, CancellationToken cancellationToken = default);
    Task<bool> DeactivateAsync(int id, CancellationToken cancellationToken = default);
}
