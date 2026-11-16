using StaffManagement.Model.Entities;

namespace StaffManagement.Core.Iservices;

public interface IPolicyDocumentService
{
    Task<IReadOnlyList<PolicyDocument>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PolicyDocument?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task AddAsync(PolicyDocument document, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
