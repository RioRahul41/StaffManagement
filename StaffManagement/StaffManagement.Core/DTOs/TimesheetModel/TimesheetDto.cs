using System.ComponentModel.DataAnnotations;
using StaffManagement.Model.Enums;

namespace StaffManagement.Core.DTOs.TimesheetModel;

public class TimesheetDto : IValidatableObject
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public string EmployeeName { get; set; } = string.Empty;

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
    public DateTime StartDate { get; set; }

    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    [Range(1, 12)]
    public int StartHour { get; set; }

    [Range(0, 59)]
    public int StartMinute { get; set; }

    [Required, RegularExpression("^(AM|PM)$")]
    public string StartPeriod { get; set; } = "AM";

    [Range(1, 12)]
    public int EndHour { get; set; }

    [Range(0, 59)]
    public int EndMinute { get; set; }

    [Required, RegularExpression("^(AM|PM)$")]
    public string EndPeriod { get; set; } = "AM";

    [EnumDataType(typeof(TimesheetStatus))]
    public TimesheetStatus Status { get; set; } = TimesheetStatus.InProgress;

    public string? DocumentPath { get; set; }

    public void ApplyTimeParts()
    {
        StartTime = ToTimeSpan(StartHour, StartMinute, StartPeriod);
        EndTime = ToTimeSpan(EndHour, EndMinute, EndPeriod);
    }

    public void SetTimeParts()
    {
        (StartHour, StartMinute, StartPeriod) = FromTimeSpan(StartTime);
        (EndHour, EndMinute, EndPeriod) = FromTimeSpan(EndTime);
    }

    private static TimeSpan ToTimeSpan(int hour, int minute, string period)
    {
        var hour24 = hour % 12 + (period == "PM" ? 12 : 0);
        return new TimeSpan(hour24, minute, 0);
    }

    private static (int Hour, int Minute, string Period) FromTimeSpan(TimeSpan time)
    {
        var hour24 = (int)time.TotalHours % 24;
        return ((hour24 % 12) is 0 ? 12 : hour24 % 12, time.Minutes,
            hour24 >= 12 ? "PM" : "AM");
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var start = ToTimeSpan(StartHour, StartMinute, StartPeriod);
        var end = ToTimeSpan(EndHour, EndMinute, EndPeriod);
        if (end <= start)
        {
            yield return new ValidationResult(
                "End time must be later than start time.",
                [nameof(EndHour), nameof(EndMinute), nameof(EndPeriod)]);
        }
    }
}
