namespace StaffManagement.Core.DTOs.LeaveModel;

public sealed class LeaveBalanceDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int SickAllowance { get; set; }
    public int SickUsed { get; set; }
    public int SickPending { get; set; }
    public int CasualAllowance { get; set; }
    public int CasualUsed { get; set; }
    public int CasualPending { get; set; }
    public int SickAvailable { get; set; }
    public int CasualAvailable { get; set; }
}
