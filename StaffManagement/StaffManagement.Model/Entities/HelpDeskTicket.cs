using System.ComponentModel.DataAnnotations;
using StaffManagement.Model.Enums;

namespace StaffManagement.Model.Entities;

public sealed class HelpDeskTicket
{
    public int Id { get; set; }

    [Required]
    public int EmployeeId { get; set; }

    public HelpDeskTicketType TicketType { get; set; }

    public DateTime Date { get; set; } = DateTime.UtcNow;

    public DateTime? UpdateDate { get; set; }

    public HelpDeskStatus Status { get; set; } = HelpDeskStatus.Open;

    [Required, StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [StringLength(500)]
    public string? DocumentPath { get; set; }

    public Employee Employee { get; set; } = null!;
}
