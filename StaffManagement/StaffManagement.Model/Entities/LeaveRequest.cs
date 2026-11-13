using System.ComponentModel.DataAnnotations;
using StaffManagement.Model.Enums;

namespace StaffManagement.Model.Entities;

public class LeaveRequest
{
    public int Id { get; set; }

    [Required]
    public int EmployeeId { get; set; }

    public LeaveType LeaveType { get; set; }

    [Required, DataType(DataType.Date)]
    public DateTime FromDate { get; set; }

    [Required, DataType(DataType.Date)]
    public DateTime ToDate { get; set; }

    [Range(1, int.MaxValue)]
    public int AppliedDays { get; set; }

    [Required, StringLength(250)]
    public string Reason { get; set; } = string.Empty;

    public LeaveStatus Status { get; set; } = LeaveStatus.Pending;

    [StringLength(500)]
    public string? DocumentPath { get; set; }

    public DateTime CreatedTime { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedTime { get; set; }

    public Employee Employee { get; set; } = null!;
}
