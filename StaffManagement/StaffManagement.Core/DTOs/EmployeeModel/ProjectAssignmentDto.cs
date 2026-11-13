namespace StaffManagement.Core.DTOs.EmployeeModel;

public sealed class ProjectAssignmentDto
{
    public int ProjectId { get; init; }
    public string ProjectName { get; init; } = string.Empty;
}
