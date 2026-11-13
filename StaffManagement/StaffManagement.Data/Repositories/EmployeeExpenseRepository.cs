using Microsoft.EntityFrameworkCore;
using StaffManagement.Core.IRepositories;
using StaffManagement.Data.DbContext;
using StaffManagement.Model.Entities;

namespace StaffManagement.Data.Repositories;

public sealed class EmployeeExpenseRepository(ApplicationDbContext dbContext) : IEmployeeExpenseRepository
{
    public async Task<(IReadOnlyList<EmployeeExpense> Items, int TotalCount)> GetPagedAsync(
        int? employeeId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.EmployeeExpenses
            .AsNoTracking()
            .Include(expense => expense.Employee)
            .Where(expense => !expense.Employee.EmpIsDeleted);

        if (employeeId.HasValue)
            query = query.Where(expense => expense.EmployeeId == employeeId.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(expense => expense.FromDate)
            .ThenByDescending(expense => expense.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<EmployeeExpense?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        dbContext.EmployeeExpenses
            .Include(expense => expense.Employee)
            .FirstOrDefaultAsync(
                expense => expense.Id == id && !expense.Employee.EmpIsDeleted,
                cancellationToken);

    public async Task AddAsync(EmployeeExpense expense, CancellationToken cancellationToken = default) =>
        await dbContext.EmployeeExpenses.AddAsync(expense, cancellationToken);

    public void Update(EmployeeExpense expense) => dbContext.EmployeeExpenses.Update(expense);

    public void Remove(EmployeeExpense expense) => dbContext.EmployeeExpenses.Remove(expense);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
