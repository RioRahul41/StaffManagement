using StaffManagement.Core.DTOs.ReliveingModel;
using StaffManagement.Model.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace StaffManagement.Core.Iservices
{
    public interface IRelievingTypeService
    {
        Task<List<RelievingTypeDto>> GetAllAsync();

        Task<List<RelievingTypeDto>> GetActiveAsync();

        Task<RelievingTypeDto?> GetByIdAsync(int id);

        Task<bool> CreateAsync(
            CreateRelievingTypeDto model,
            string createdBy);

        Task<bool> UpdateAsync(
            UpdateRelievingTypeDto model,
            string updatedBy);

        Task<bool> SoftDeleteAsync(
            int id,
            string updatedBy);

        Task<bool> UpdateStatusAsync(
            int id,
            bool isActive,
            string updatedBy);
    
}
}
