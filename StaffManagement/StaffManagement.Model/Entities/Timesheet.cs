using System.ComponentModel.DataAnnotations;
using StaffManagement.Model.Enums;

namespace StaffManagement.Model.Entities;

public class Timesheet
{
    public int Id { get; set; }

    [Required]
    public int EmployeeId { get; set; }

    public int? ProjectId { get; set; }

    public Project? Project { get; set; }

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
    public DateTime StartDate { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Start time is required.")]
    [DataType(DataType.Time)]
    public TimeSpan StartTime { get; set; }

    [Required(ErrorMessage = "End time is required.")]
    [DataType(DataType.Time)]
    public TimeSpan EndTime { get; set; }

    public TimesheetStatus Status { get; set; } = TimesheetStatus.InProgress;

    [StringLength(500)]
    public string? DocumentPath { get; set; }

    public DateTime CreatedTime { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedTime { get; set; }

    public Employee Employee { get; set; } = null!;
}
