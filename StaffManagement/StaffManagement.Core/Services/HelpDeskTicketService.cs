using StaffManagement.Core.DTOs;
using StaffManagement.Core.DTOs.HelpDeskModel;
using StaffManagement.Core.IRepositories;
using StaffManagement.Core.Iservices;
using StaffManagement.Model.Entities;
using StaffManagement.Model.Enums;

namespace StaffManagement.Core.Services;

public sealed class HelpDeskTicketService(
    IHelpDeskTicketRepository ticketRepository,
    IEmployeeRepository employeeRepository) : IHelpDeskTicketService
{
    public async Task<PagedResultDto<HelpDeskTicketDto>> GetPagedAsync(
        string identityUserId,
        bool canViewAll,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 100);

        int? employeeId = null;
        if (!canViewAll)
        {
            var employee = await employeeRepository.GetByIdentityUserIdAsync(identityUserId, cancellationToken);
            if (employee is null)
                return new PagedResultDto<HelpDeskTicketDto> { PageNumber = pageNumber, PageSize = pageSize };
            employeeId = employee.Id;
        }

        var result = await ticketRepository.GetPagedAsync(employeeId, pageNumber, pageSize, cancellationToken);
        return new PagedResultDto<HelpDeskTicketDto>
        {
            Items = result.Items.Select(Map).ToList(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = result.TotalCount
        };
    }

    public async Task<(bool Succeeded, string? Error)> CreateAsync(
        CreateHelpDeskTicketDto model,
        string identityUserId,
        string? documentPath,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.IsDefined(model.TicketType))
            return (false, "Select a valid ticket type.");
        var employee = await employeeRepository.GetByIdentityUserIdAsync(identityUserId, cancellationToken);
        if (employee is null)
            return (false, "Your employee profile could not be found.");

        var ticket = new HelpDeskTicket
        {
            EmployeeId = employee.Id,
            TicketType = model.TicketType,
            Date = DateTime.UtcNow,
            Status = HelpDeskStatus.Open,
            Description = model.Description.Trim(),
            DocumentPath = documentPath
        };

        await ticketRepository.AddAsync(ticket, cancellationToken);
        await ticketRepository.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    public async Task<(bool Succeeded, string? Error)> UpdateStatusAsync(
        int id,
        HelpDeskStatus status,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.IsDefined(status))
            return (false, "Select a valid status.");

        var ticket = await ticketRepository.GetByIdAsync(id, cancellationToken);
        if (ticket is null)
            return (false, "Help Desk ticket was not found.");

        ticket.Status = status;
        ticket.UpdateDate = DateTime.UtcNow;
        ticketRepository.Update(ticket);
        await ticketRepository.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    public async Task<string?> GetDocumentPathAsync(
        int id,
        string identityUserId,
        bool canViewAll,
        CancellationToken cancellationToken = default)
    {
        var ticket = await ticketRepository.GetByIdAsync(id, cancellationToken);
        if (ticket is null || string.IsNullOrWhiteSpace(ticket.DocumentPath))
            return null;

        return canViewAll || ticket.Employee.IdentityUserId == identityUserId
            ? ticket.DocumentPath
            : null;
    }

    private static HelpDeskTicketDto Map(HelpDeskTicket ticket) => new()
    {
        Id = ticket.Id,
        FirstName = ticket.Employee.FirstName,
        TicketType = ticket.TicketType,
        Date = ticket.Date,
        UpdateDate = ticket.UpdateDate,
        Status = ticket.Status,
        Description = ticket.Description,
        DocumentPath = ticket.DocumentPath
    };
}
