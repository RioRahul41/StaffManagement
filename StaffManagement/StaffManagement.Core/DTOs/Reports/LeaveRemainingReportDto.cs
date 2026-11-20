namespace StaffManagement.Core.DTOs.Reports;

public sealed class LeaveRemainingReportDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string LeaveType { get; set; } = string.Empty;
    public int Allowance { get; set; }
    public int Used { get; set; }
    public int Pending { get; set; }
    public int Remaining { get; set; }
}
