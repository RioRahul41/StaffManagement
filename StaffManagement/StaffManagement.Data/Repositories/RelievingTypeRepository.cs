
using Microsoft.EntityFrameworkCore;
using StaffManagement.Core.IRepositories;
using StaffManagement.Data.DbContext;
using StaffManagement.Model.Entities;

namespace StaffManagement.Data.Repositories
{
    public class RelievingTypeRepository : IRelievingTypeRepository
    {
        private readonly ApplicationDbContext _context;

        public RelievingTypeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get all non-deleted relieving types
        public async Task<List<RelievingType>> GetAllAsync()
        {
            return await _context.RelievingTypes
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.RelievingTypeName)
                .ToListAsync();
        }

        // Get active relieving types for Employee dropdown
        public async Task<List<RelievingType>> GetActiveAsync()
        {
            return await _context.RelievingTypes
                .Where(x => x.IsActive && !x.IsDeleted)
                .OrderBy(x => x.RelievingTypeName)
                .ToListAsync();
        }

        // Get relieving type by Id
        public async Task<RelievingType?> GetByIdAsync(int id)
        {
            return await _context.RelievingTypes
                .FirstOrDefaultAsync(x =>
                    x.Id == id && !x.IsDeleted);
        }

        // Check duplicate relieving type name
        public async Task<bool> ExistsByNameAsync(
            string name,
            int? excludeId = null)
        {
            var normalizedName = name.Trim().ToLower();

            var query = _context.RelievingTypes
                .Where(x => !x.IsDeleted &&
                    x.RelievingTypeName.ToLower() == normalizedName);

            if (excludeId.HasValue)
            {
                query = query.Where(x => x.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        // Create relieving type
        public async Task AddAsync(RelievingType relievingType)
        {
            await _context.RelievingTypes.AddAsync(relievingType);
            await _context.SaveChangesAsync();
        }

        // Update relieving type
        public async Task UpdateAsync(RelievingType relievingType)
        {
            relievingType.UpdatedAt = DateTime.Now;

            _context.RelievingTypes.Update(relievingType);

            await _context.SaveChangesAsync();
        }

        // Soft delete relieving type
        public async Task<bool> SoftDeleteAsync(int id)
        {
            var relievingType = await GetByIdAsync(id);

            if (relievingType == null)
                return false;

            // Prevent deleting a type already used by employees
            var isAssigned = await _context.Employees
                .AnyAsync(x => x.RelievingTypeId == id);

            if (isAssigned)
                return false;

            relievingType.IsDeleted = true;
            relievingType.IsActive = false;
            relievingType.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return true;
        }

        // Activate / Deactivate relieving type
        public async Task<bool> UpdateStatusAsync(int id, bool isActive)
        {
            var relievingType = await GetByIdAsync(id);

            if (relievingType == null)
                return false;

            relievingType.IsActive = isActive;
            relievingType.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return true;
        }
    }
}
