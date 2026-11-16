using Microsoft.EntityFrameworkCore;
using StaffManagement.Core.IRepositories;
using StaffManagement.Data.DbContext;
using StaffManagement.Model.Entities;

namespace StaffManagement.Data.Repositories;

public sealed class HelpDeskTicketRepository(ApplicationDbContext dbContext) : IHelpDeskTicketRepository
{
    public async Task<(IReadOnlyList<HelpDeskTicket> Items, int TotalCount)> GetPagedAsync(
        int? employeeId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.HelpDeskTickets
            .AsNoTracking()
            .Include(ticket => ticket.Employee)
            .Where(ticket => !ticket.Employee.EmpIsDeleted);

        if (employeeId.HasValue)
            query = query.Where(ticket => ticket.EmployeeId == employeeId.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(ticket => ticket.Date)
            .ThenByDescending(ticket => ticket.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<HelpDeskTicket?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        dbContext.HelpDeskTickets
            .Include(ticket => ticket.Employee)
            .FirstOrDefaultAsync(ticket => ticket.Id == id && !ticket.Employee.EmpIsDeleted, cancellationToken);

    public async Task AddAsync(HelpDeskTicket ticket, CancellationToken cancellationToken = default) =>
        await dbContext.HelpDeskTickets.AddAsync(ticket, cancellationToken);

    public void Update(HelpDeskTicket ticket) => dbContext.HelpDeskTickets.Update(ticket);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
