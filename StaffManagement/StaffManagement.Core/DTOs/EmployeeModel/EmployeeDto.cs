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

    [EmailAddress, StringLength(256)]
    public string? PersonalEmail { get; set; }

    [StringLength(1000)]
    public string? Skills { get; set; }

    public string? ProfilePhotoPath { get; set; }

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

    public DateTime CreatedTime { get; set; }

    [DataType(DataType.Date)]
    public DateTime? DOB { get; set; }

    [Range(typeof(decimal), "0.01", "999999999")]
    public decimal? Salary { get; set; }

    [StringLength(100)]
    public string? Designation { get; set; }

    public Gender? Gender { get; set; }
    [StringLength(30)]
    public string? MaritalStatus { get; set; }
    [StringLength(5)]
    public string? BloodGroup { get; set; }
    [StringLength(100)]
    public string? EmergencyContactName { get; set; }
    [StringLength(50)]
    public string? EmergencyContactRelationship { get; set; }
    [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Emergency contact phone number must contain exactly 10 digits.")]
    public string? EmergencyContactPhone { get; set; }
    public bool EmpIsActive { get; set; } = true;

    [Range(0, 365, ErrorMessage = "Sick leave allowance must be between 0 and 365 days.")]
    public int SickLeaveAllowance { get; set; } = 10;

    [Range(0, 365, ErrorMessage = "Casual leave allowance must be between 0 and 365 days.")]
    public int CasualLeaveAllowance { get; set; } = 10;
}

