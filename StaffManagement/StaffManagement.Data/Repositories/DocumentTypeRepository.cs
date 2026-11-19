using Microsoft.EntityFrameworkCore;
using StaffManagement.Core.IRepositories;
using StaffManagement.Data.DbContext;
using StaffManagement.Model.Entities;

namespace StaffManagement.Data.Repositories;

public sealed class DocumentTypeRepository(ApplicationDbContext dbContext) : IDocumentTypeRepository
{
    public async Task<IReadOnlyList<DocumentType>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await dbContext.DocumentTypes.AsNoTracking()
            .OrderBy(type => !type.IsActive)
            .ThenBy(type => type.DocumentTypeName)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<DocumentType>> GetActiveAsync(CancellationToken cancellationToken = default) =>
        await dbContext.DocumentTypes.AsNoTracking()
            .Where(type => type.IsActive)
            .OrderBy(type => type.DocumentTypeName)
            .ToListAsync(cancellationToken);

    public Task<DocumentType?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        dbContext.DocumentTypes.FirstOrDefaultAsync(type => type.DocumentTypeId == id, cancellationToken);

    public Task<bool> ExistsByNameAsync(string name, int? excludedId = null, CancellationToken cancellationToken = default)
    {
        var normalizedName = name.Trim().ToLower();
        return dbContext.DocumentTypes.AnyAsync(type =>
            type.DocumentTypeName.ToLower() == normalizedName &&
            (!excludedId.HasValue || type.DocumentTypeId != excludedId.Value), cancellationToken);
    }

    public async Task AddAsync(DocumentType documentType, CancellationToken cancellationToken = default) =>
        await dbContext.DocumentTypes.AddAsync(documentType, cancellationToken);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
