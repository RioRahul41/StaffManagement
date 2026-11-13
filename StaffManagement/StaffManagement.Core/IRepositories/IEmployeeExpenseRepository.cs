using StaffManagement.Model.Entities;

namespace StaffManagement.Core.IRepositories;

public interface IEmployeeExpenseRepository
{
    Task<(IReadOnlyList<EmployeeExpense> Items, int TotalCount)> GetPagedAsync(
        int? employeeId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<EmployeeExpense?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task AddAsync(EmployeeExpense expense, CancellationToken cancellationToken = default);
    void Update(EmployeeExpense expense);
    void Remove(EmployeeExpense expense);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
