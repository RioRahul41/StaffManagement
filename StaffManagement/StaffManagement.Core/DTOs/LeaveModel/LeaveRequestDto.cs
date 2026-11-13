using StaffManagement.Model.Enums;

namespace StaffManagement.Core.DTOs.LeaveModel;

public class LeaveRequestDto
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public string EmployeeName { get; set; } = string.Empty;

    public LeaveType LeaveType { get; set; }

    public DateTime FromDate { get; set; }

    public DateTime ToDate { get; set; }

    public int AppliedDays { get; set; }

    public string Reason { get; set; } = string.Empty;

    public LeaveStatus Status { get; set; }

    public string? DocumentPath { get; set; }

    public bool CanReview { get; set; }
}
