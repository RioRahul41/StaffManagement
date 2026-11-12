namespace StaffManagement.Core.DTOs;

public sealed class DashboardViewModel
{
    public string Role { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public int TotalEmployees { get; init; }
    public int ActiveEmployees { get; init; }
    public int InactiveEmployees => TotalEmployees - ActiveEmployees;
    public bool CanCreateAccounts { get; init; }
    public bool CanManageEmployees { get; init; }
}
