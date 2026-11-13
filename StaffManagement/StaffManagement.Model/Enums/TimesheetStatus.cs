using System.ComponentModel.DataAnnotations;

namespace StaffManagement.Model.Enums;

public enum TimesheetStatus
{
    Pending,

    [Display(Name = "In Progress")]
    InProgress,
    Completed
}
