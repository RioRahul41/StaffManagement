namespace StaffManagement.Core.DTOs;

public sealed class EmployeeCelebrationGreetingDto
{
    public string EmployeeName { get; init; } = string.Empty;
    public string? Designation { get; init; }
    public bool HasBirthday { get; init; }
    public bool HasWorkAnniversary { get; init; }
    public int CompletedYears { get; init; }
}
