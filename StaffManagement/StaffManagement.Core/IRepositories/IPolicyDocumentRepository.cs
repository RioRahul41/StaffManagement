using StaffManagement.Model.Entities;

namespace StaffManagement.Core.IRepositories;

public interface IPolicyDocumentRepository
{
    Task<IReadOnlyList<PolicyDocument>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PolicyDocument?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task AddAsync(PolicyDocument document, CancellationToken cancellationToken = default);
    Task DeleteAsync(PolicyDocument document, CancellationToken cancellationToken = default);
}
