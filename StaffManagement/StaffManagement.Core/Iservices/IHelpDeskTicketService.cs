using StaffManagement.Core.DTOs;
using StaffManagement.Core.DTOs.HelpDeskModel;

namespace StaffManagement.Core.Iservices;

public interface IHelpDeskTicketService
{
    Task<PagedResultDto<HelpDeskTicketDto>> GetPagedAsync(
        string identityUserId,
        bool canViewAll,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<(bool Succeeded, string? Error)> CreateAsync(
        CreateHelpDeskTicketDto model,
        string identityUserId,
        string? documentPath,
        CancellationToken cancellationToken = default);

    Task<string?> GetDocumentPathAsync(
        int id,
        string identityUserId,
        bool canViewAll,
        CancellationToken cancellationToken = default);

    Task<(bool Succeeded, string? Error)> UpdateStatusAsync(
        int id,
        StaffManagement.Model.Enums.HelpDeskStatus status,
        CancellationToken cancellationToken = default);
}
