using Microsoft.EntityFrameworkCore;
using StaffManagement.Core.IRepositories;
using StaffManagement.Data.DbContext;
using StaffManagement.Model.Entities;

namespace StaffManagement.Data.Repositories;

public sealed class PolicyDocumentRepository(ApplicationDbContext context) : IPolicyDocumentRepository
{
    public async Task<IReadOnlyList<PolicyDocument>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await context.PolicyDocuments
            .AsNoTracking()
            .OrderBy(document => document.PolicyName)
            .ThenByDescending(document => document.UploadedAtUtc)
            .ToListAsync(cancellationToken);

    public Task<PolicyDocument?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        context.PolicyDocuments.FirstOrDefaultAsync(document => document.Id == id, cancellationToken);

    public async Task AddAsync(PolicyDocument document, CancellationToken cancellationToken = default)
    {
        await context.PolicyDocuments.AddAsync(document, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(PolicyDocument document, CancellationToken cancellationToken = default)
    {
        context.PolicyDocuments.Remove(document);
        await context.SaveChangesAsync(cancellationToken);
    }
}
