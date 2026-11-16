using StaffManagement.Model.Enums;

namespace StaffManagement.Core.DTOs.HelpDeskModel;

public sealed class HelpDeskTicketDto
{
    public int Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public HelpDeskTicketType TicketType { get; init; }
    public DateTime Date { get; init; }
    public DateTime? UpdateDate { get; init; }
    public HelpDeskStatus Status { get; init; }
    public string Description { get; init; } = string.Empty;
    public string? DocumentPath { get; init; }
}
