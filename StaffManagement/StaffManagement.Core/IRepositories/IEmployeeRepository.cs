using StaffManagement.Model.Entities;

namespace StaffManagement.Core.IRepositories
{
    public interface IEmployeeRepository
    {
        Task<IReadOnlyList<Employee>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<(IReadOnlyList<Employee> Items, int TotalCount)> GetPageAsync(
            string? searchTerm,
            int? departmentId,
            string? designation,
            bool? isActive,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default);
        Task<Employee?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Employee?> GetByIdentityUserIdAsync(string identityUserId, CancellationToken cancellationToken = default);
        Task AddAsync(Employee employee, CancellationToken cancellationToken = default);
        void Update(Employee employee);
        void Remove(Employee employee);
        Task<bool> EmailExistsAsync(string email, int? excludedId = null, CancellationToken cancellationToken = default);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
