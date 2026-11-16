using StaffManagement.Model.Entities;

namespace StaffManagement.Core.IRepositories;

public interface IHelpDeskTicketRepository
{
    Task<(IReadOnlyList<HelpDeskTicket> Items, int TotalCount)> GetPagedAsync(
        int? employeeId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<HelpDeskTicket?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task AddAsync(HelpDeskTicket ticket, CancellationToken cancellationToken = default);
    void Update(HelpDeskTicket ticket);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
