using StaffManagement.Core.IRepositories;
using StaffManagement.Core.Iservices;
using StaffManagement.Model.Entities;

namespace StaffManagement.Core.Services;

public sealed class PolicyDocumentService(IPolicyDocumentRepository repository) : IPolicyDocumentService
{
    public Task<IReadOnlyList<PolicyDocument>> GetAllAsync(CancellationToken cancellationToken = default) =>
        repository.GetAllAsync(cancellationToken);

    public Task<PolicyDocument?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        repository.GetByIdAsync(id, cancellationToken);

    public Task AddAsync(PolicyDocument document, CancellationToken cancellationToken = default) =>
        repository.AddAsync(document, cancellationToken);

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var document = await repository.GetByIdAsync(id, cancellationToken);
        if (document is null)
            return false;

        await repository.DeleteAsync(document, cancellationToken);
        return true;
    }
}
