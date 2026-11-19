using StaffManagement.Model.Entities;

namespace StaffManagement.Core.IRepositories;

public interface IDocumentTypeRepository
{
    Task<IReadOnlyList<DocumentType>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DocumentType>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<DocumentType?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, int? excludedId = null, CancellationToken cancellationToken = default);
    Task AddAsync(DocumentType documentType, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
