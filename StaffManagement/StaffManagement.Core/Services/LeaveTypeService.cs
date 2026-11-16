using StaffManagement.Core.IRepositories;
using StaffManagement.Core.Iservices;
using StaffManagement.Model.Entities;

namespace StaffManagement.Core.Services;

public sealed class LeaveTypeService(ILeaveTypeRepository leaveTypeRepository) : ILeaveTypeService
{
    public Task<IReadOnlyList<LeaveTypeMaster>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default) =>
        leaveTypeRepository.GetAllAsync(includeInactive, cancellationToken);

    public Task<LeaveTypeMaster?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        leaveTypeRepository.GetByIdAsync(id, cancellationToken);

    public async Task<(bool Succeeded, string? Error)> CreateAsync(LeaveTypeMaster leaveType, CancellationToken cancellationToken = default)
    {
        leaveType.Name = leaveType.Name.Trim();
        if (string.IsNullOrWhiteSpace(leaveType.Name)) return (false, "Leave type name is required.");
        if (await leaveTypeRepository.ExistsByNameAsync(leaveType.Name, cancellationToken: cancellationToken))
            return (false, "A leave type with this name already exists.");
        leaveType.IsActive = true;
        leaveType.CreatedDate = DateTime.UtcNow;
        await leaveTypeRepository.AddAsync(leaveType, cancellationToken);
        await leaveTypeRepository.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    public async Task<(bool Succeeded, string? Error)> UpdateAsync(LeaveTypeMaster leaveType, CancellationToken cancellationToken = default)
    {
        var existing = await leaveTypeRepository.GetByIdAsync(leaveType.Id, cancellationToken);
        if (existing is null) return (false, "Leave type was not found.");
        var name = leaveType.Name.Trim();
        if (string.IsNullOrWhiteSpace(name)) return (false, "Leave type name is required.");
        if (await leaveTypeRepository.ExistsByNameAsync(name, leaveType.Id, cancellationToken))
            return (false, "A leave type with this name already exists.");
        existing.Name = name;
        existing.UpdatedDate = DateTime.UtcNow;
        leaveTypeRepository.Update(existing);
        await leaveTypeRepository.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var leaveType = await leaveTypeRepository.GetByIdAsync(id, cancellationToken);
        if (leaveType is null || !leaveType.IsActive) return false;
        leaveType.IsActive = false;
        leaveType.UpdatedDate = DateTime.UtcNow;
        leaveTypeRepository.Update(leaveType);
        await leaveTypeRepository.SaveChangesAsync(cancellationToken);
        return true;
    }
}
