using StaffManagement.Model.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace StaffManagement.Core.IRepositories
{
    public interface IRelievingTypeRepository
    {

        Task<List<RelievingType>> GetAllAsync();

        Task<List<RelievingType>> GetActiveAsync();

        Task<RelievingType?> GetByIdAsync(int id);

        Task<bool> ExistsByNameAsync(
            string name,
            int? excludeId = null);

        Task AddAsync(RelievingType relievingType);

        Task UpdateAsync(RelievingType relievingType);

        Task<bool> SoftDeleteAsync(int id);

        Task<bool> UpdateStatusAsync(int id, bool isActive);

    }
}
