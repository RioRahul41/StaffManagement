using StaffManagement.Model.Enums;

namespace StaffManagement.Core.DTOs.TimesheetModel;

public sealed class TimesheetLogReportDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string TaskName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public TimesheetStatus Status { get; set; }
}
