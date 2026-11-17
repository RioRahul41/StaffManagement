namespace StaffManagement.Core.DTOs.EmployeeModel;

public sealed class EmployeeDataTableQueryDto
{
    public int Start { get; init; }
    public int Length { get; init; } = 10;
    public string? SearchTerm { get; init; }
    public string? SortColumn { get; init; }
    public bool SortAscending { get; init; }
    public int? DepartmentId { get; init; }
    public string? Designation { get; init; }
    public bool? IsActive { get; init; }
}
