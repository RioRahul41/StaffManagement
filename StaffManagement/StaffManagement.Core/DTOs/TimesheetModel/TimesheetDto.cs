using System.ComponentModel.DataAnnotations;
using StaffManagement.Model.Enums;

namespace StaffManagement.Core.DTOs.TimesheetModel;

public class TimesheetDto : IValidatableObject
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public string EmployeeName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Project name is required.")]
    [StringLength(150, ErrorMessage = "Project name cannot exceed 150 characters.")]
    public string ProjectName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Task name is required.")]
    [StringLength(150, ErrorMessage = "Task name cannot exceed 150 characters.")]
    public string TaskName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Start date is required.")]
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "Start time is required.")]
    [DataType(DataType.Time)]
    public TimeSpan StartTime { get; set; }

    [Required(ErrorMessage = "End time is required.")]
    [DataType(DataType.Time)]
    public TimeSpan EndTime { get; set; }

    public TimesheetStatus Status { get; set; } = TimesheetStatus.Pending;

    public string? DocumentPath { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (EndTime <= StartTime)
        {
            yield return new ValidationResult(
                "End time must be later than start time.",
                [nameof(EndTime)]);
        }
    }
}
