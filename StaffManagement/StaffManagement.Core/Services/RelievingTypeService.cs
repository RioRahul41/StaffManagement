using StaffManagement.Core.DTOs.ReliveingModel;
using StaffManagement.Core.IRepositories;
using StaffManagement.Core.Iservices;
using StaffManagement.Model.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace StaffManagement.Core.Services
{
       public class RelievingTypeService : IRelievingTypeService
        {
            private readonly IRelievingTypeRepository _repository;

            public RelievingTypeService(
                IRelievingTypeRepository repository)
            {
                _repository = repository;
            }

        public async Task<List<RelievingTypeDto>> GetAllAsync()
        {
            var types = await _repository.GetAllAsync();

            return types.Select(x => new RelievingTypeDto
            {
                Id = x.Id,
                RelievingTypeName = x.RelievingTypeName,
                IsActive = x.IsActive,
                CreatedBy = x.CreatedBy,
                CreatedAt = x.CreatedAt,
                UpdatedBy = x.UpdatedBy,
                UpdatedAt = x.UpdatedAt
            }).ToList();
        }

        public async Task<List<RelievingTypeDto>> GetActiveAsync()
            {
                var types = await _repository.GetActiveAsync();

                return types.Select(x => new RelievingTypeDto
                {
                    Id = x.Id,
                    RelievingTypeName = x.RelievingTypeName,
                    IsActive = x.IsActive
                }).ToList();
            }

            public async Task<RelievingTypeDto?> GetByIdAsync(int id)
            {
                var type = await _repository.GetByIdAsync(id);

                if (type == null)
                    return null;

                return new RelievingTypeDto
                {
                    Id = type.Id,
                    RelievingTypeName = type.RelievingTypeName,
                    IsActive = type.IsActive,
                    CreatedBy = type.CreatedBy,
                    CreatedAt = type.CreatedAt,
                    UpdatedBy = type.UpdatedBy,
                    UpdatedAt = type.UpdatedAt
                };
            }

            public async Task<bool> CreateAsync(
                CreateRelievingTypeDto model,
                string createdBy)
            {
                var name = model.RelievingTypeName?.Trim();

                if (string.IsNullOrWhiteSpace(name))
                    return false;

                if (await _repository.ExistsByNameAsync(name))
                    return false;

                var relievingType = new RelievingType
                {
                    RelievingTypeName = name,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedBy = createdBy,
                    CreatedAt = DateTime.Now
                };

                await _repository.AddAsync(relievingType);

                return true;
            }

            public async Task<bool> UpdateAsync(
                UpdateRelievingTypeDto model,
                string updatedBy)
            {
                var name = model.RelievingTypeName?.Trim();

                if (string.IsNullOrWhiteSpace(name))
                    return false;

                var relievingType = await _repository.GetByIdAsync(model.Id);

                if (relievingType == null)
                    return false;

                if (await _repository.ExistsByNameAsync(name, model.Id))
                    return false;

                relievingType.RelievingTypeName = name;
                relievingType.IsActive = model.IsActive;
                relievingType.UpdatedBy = updatedBy;
                relievingType.UpdatedAt = DateTime.Now;

                await _repository.UpdateAsync(relievingType);

                return true;
            }

        public async Task<bool> SoftDeleteAsync(int id, string updatedBy)
        {
            var relievingType = await _repository.GetByIdAsync(id);

            if (relievingType == null)
                return false;

            relievingType.IsDeleted = true;
            relievingType.IsActive = false;
            relievingType.UpdatedBy = updatedBy;
            relievingType.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(relievingType);

            return true;
        }

        public async Task<bool> UpdateStatusAsync(
                int id,
                bool isActive,
                string updatedBy)
            {
                var relievingType = await _repository.GetByIdAsync(id);

                if (relievingType == null)
                    return false;

                relievingType.IsActive = isActive;
                relievingType.UpdatedBy = updatedBy;
                relievingType.UpdatedAt = DateTime.Now;

                await _repository.UpdateAsync(relievingType);

                return true;
            }
        }
}

