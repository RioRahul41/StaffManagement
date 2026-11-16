using System.ComponentModel.DataAnnotations;
namespace StaffManagement.Core.DTOs.LeaveModel;

public class CreateLeaveRequestDto : IValidatableObject
{
    [Required, StringLength(100)]
    public string LeaveType { get; set; } = string.Empty;

    public bool IsSickLeave => string.Equals(LeaveType, "Sick", StringComparison.OrdinalIgnoreCase);

    [Required, DataType(DataType.Date)]
    public DateTime FromDate { get; set; } = DateTime.Today;

    [Required, DataType(DataType.Date)]
    public DateTime ToDate { get; set; } = DateTime.Today;

    [Required, StringLength(250, ErrorMessage = "Reason cannot exceed 250 characters.")]
    public string Reason { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (IsSickLeave)
        {
            if (FromDate.Date > DateTime.Today || ToDate.Date > DateTime.Today)
                yield return new ValidationResult("Sick leave can only be requested for today or a past date.", [nameof(FromDate), nameof(ToDate)]);
        }
        else if (FromDate.Date < DateTime.Today || ToDate.Date < DateTime.Today)
        {
            yield return new ValidationResult("Leave can only be applied for today or a future date.", [nameof(FromDate), nameof(ToDate)]);
        }

        if (ToDate.Date < FromDate.Date)
            yield return new ValidationResult("To date must be on or after from date.", [nameof(ToDate)]);
    }
}
