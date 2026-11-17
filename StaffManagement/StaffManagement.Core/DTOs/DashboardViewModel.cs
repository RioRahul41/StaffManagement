using StaffManagement.Core.DTOs.EmployeeModel;

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
    public IReadOnlyList<ProjectAssignmentDto> CurrentProjects { get; init; } = [];
    public IReadOnlyList<DashboardBirthdayItem> MonthlyBirthdays { get; init; } = [];
    public IReadOnlyList<DashboardHolidayItem> UpcomingHolidays { get; init; } = [];
    public IReadOnlyList<DashboardWorkAnniversaryItem> MonthlyWorkAnniversaries { get; init; } = [];
}

public sealed class DashboardBirthdayItem
{
    public string EmployeeName { get; init; } = string.Empty;
    public string? Designation { get; init; }
    public DateTime Birthday { get; init; }
    public bool IsCelebrated { get; init; }
    public bool IsToday { get; init; }
}

public sealed class DashboardHolidayItem
{
    public string HolidayName { get; init; } = string.Empty;
    public DateTime HolidayDate { get; init; }
}

public sealed class DashboardWorkAnniversaryItem
{
    public string EmployeeName { get; init; } = string.Empty;
    public string? Designation { get; init; }
    public DateTime AnniversaryDate { get; init; }
    public int CompletedYears { get; init; }
}
