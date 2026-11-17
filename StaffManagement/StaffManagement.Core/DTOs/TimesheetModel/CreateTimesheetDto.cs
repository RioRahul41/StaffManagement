using System.ComponentModel.DataAnnotations;
using StaffManagement.Model.Enums;

namespace StaffManagement.Core.DTOs.TimesheetModel;

public class CreateTimesheetDto : IValidatableObject
{
    public int? EmployeeId { get; set; }

    [Required(ErrorMessage = "Please select a project.")]
    public int? ProjectId { get; set; }

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
    public DateTime? StartDate { get; set; }

    [Required(ErrorMessage = "Start time is required.")]
    [DataType(DataType.Time)]
    public TimeSpan? StartTime { get; set; }

    [Required(ErrorMessage = "End time is required.")]
    [DataType(DataType.Time)]
    public TimeSpan? EndTime { get; set; }

    [Required(ErrorMessage = "Select a work status.")]
    [EnumDataType(typeof(TimesheetStatus))]
    public TimesheetStatus? Status { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (StartTime.HasValue && !IsAllowedTime(StartTime.Value))
        {
            yield return new ValidationResult(
                "Start time must be between 12:00 AM and 11:30 PM in 30-minute intervals.",
                [nameof(StartTime)]);
        }

        if (EndTime.HasValue && !IsAllowedTime(EndTime.Value))
        {
            yield return new ValidationResult(
                "End time must be between 12:00 AM and 11:30 PM in 30-minute intervals.",
                [nameof(EndTime)]);
        }

        if (StartTime.HasValue && EndTime.HasValue && EndTime.Value <= StartTime.Value)
        {
            yield return new ValidationResult(
                "End time must be later than start time.",
                [nameof(EndTime)]);
        }
    }

    private static bool IsAllowedTime(TimeSpan time) =>
        time >= TimeSpan.Zero &&
        time <= new TimeSpan(23, 30, 0) &&
        time.Ticks % TimeSpan.FromMinutes(30).Ticks == 0;
}
