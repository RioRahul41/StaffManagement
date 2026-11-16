using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using StaffManagement.Core.IRepositories;
using StaffManagement.Data.DbContext;
using StaffManagement.Model.Entities;

namespace StaffManagement.Data.Repositories;

public sealed class EmployeeExpenseRepository(ApplicationDbContext dbContext) : IEmployeeExpenseRepository
{
    public async Task<(IReadOnlyList<EmployeeExpenseGroup> Groups, int TotalCount)> GetPagedGroupsAsync(
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

        var groupedQuery = query
            .GroupBy(expense => new { expense.EmployeeId, expense.Title, expense.FromDate, expense.ToDate })
            .Select(group => new EmployeeExpenseGroup
            {
                EmployeeId = group.Key.EmployeeId,
                Title = group.Key.Title,
                FromDate = group.Key.FromDate,
                ToDate = group.Key.ToDate,
                BillAmount = group.Sum(expense => expense.BillAmount)
            });

        var totalCount = await groupedQuery.CountAsync(cancellationToken);
        var groups = await groupedQuery
            .OrderByDescending(group => group.FromDate)
            .ThenBy(group => group.EmployeeId)
            .ThenBy(group => group.Title)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        if (groups.Count == 0)
            return (groups, totalCount);

        var parameter = Expression.Parameter(typeof(EmployeeExpense), "expense");
        Expression predicateBody = Expression.Constant(false);
        foreach (var group in groups)
        {
            Expression keyMatch = Expression.AndAlso(
                Expression.Equal(Expression.Property(parameter, nameof(EmployeeExpense.EmployeeId)), Expression.Constant(group.EmployeeId)),
                Expression.AndAlso(
                    Expression.Equal(Expression.Property(parameter, nameof(EmployeeExpense.Title)), Expression.Constant(group.Title)),
                    Expression.AndAlso(
                        Expression.Equal(Expression.Property(parameter, nameof(EmployeeExpense.FromDate)), Expression.Constant(group.FromDate)),
                        Expression.Equal(Expression.Property(parameter, nameof(EmployeeExpense.ToDate)), Expression.Constant(group.ToDate)))));
            predicateBody = Expression.OrElse(predicateBody, keyMatch);
        }

        var predicate = Expression.Lambda<Func<EmployeeExpense, bool>>(predicateBody, parameter);
        var expenses = await query
            .Where(predicate)
            .ToListAsync(cancellationToken);

        foreach (var group in groups)
        {
            group.Expenses = expenses
                .Where(expense => expense.EmployeeId == group.EmployeeId
                    && expense.Title == group.Title
                    && expense.FromDate == group.FromDate
                    && expense.ToDate == group.ToDate)
                .OrderBy(expense => expense.Id)
                .ToList();
            group.Status = group.Expenses[0].Status;
            group.ApprovalNotes = group.Expenses[0].ApprovalNotes;
        }

        return (groups, totalCount);
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
