using StaffManagement.Core.DTOs.EmployeeModel;
using StaffManagement.Model.Entities;

namespace StaffManagement.Models;

public sealed class EmployeeListViewModel
{
    public IReadOnlyList<EmployeeDto> Employees { get; init; } = [];
    public IReadOnlyList<Department> Departments { get; init; } = [];
    public IReadOnlyList<Designation> Designations { get; init; } = [];
    public string? SearchTerm { get; init; }
    public int? DepartmentId { get; init; }
    public string? Designation { get; init; }
    public bool? IsActive { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)Math.Max(1, PageSize)));
}
