using System.ComponentModel.DataAnnotations;
using StaffManagement.Model.Enums;

namespace StaffManagement.Core.DTOs.EmployeeModel;

public class EmployeeDto
{
    public int Id { get; set; }
    public string? IdentityUserId { get; set; }

    public int? ReportingPersonId { get; set; }

    public string? ReportingPersonName { get; set; }

    [Required, StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Phone number must contain exactly 10 digits.")]
    public string? Phone { get; set; }

    [Required(ErrorMessage = "Department is required.")]
    public int? DepartmentId { get; set; }

    public List<int> ProjectIds { get; set; } = [];

    public List<ProjectAssignmentDto> AssignedProjects { get; set; } = [];

    [StringLength(100)]
    public string? Department { get; set; }

    [Required, StringLength(100)]
    public string Location { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    public DateTime? DOB { get; set; }

    [Range(typeof(decimal), "0.01", "999999999")]
    public decimal? Salary { get; set; }

    [StringLength(100)]
    public string? Designation { get; set; }

    public Gender? Gender { get; set; }
    public bool EmpIsActive { get; set; } = true;
}
