using System.ComponentModel.DataAnnotations;
namespace StaffManagement.Core.DTOs.LeaveModel;

public class CreateLeaveRequestDto : IValidatableObject
{
    [Required, StringLength(100)]
    public string LeaveType { get; set; } = string.Empty;

    public bool IsSickLeave => string.Equals(LeaveType, "Sick", StringComparison.OrdinalIgnoreCase);

    [Required, DataType(DataType.Date)]
    public DateTime? FromDate { get; set; }

    [Required, DataType(DataType.Date)]
    public DateTime? ToDate { get; set; }

    [Required, StringLength(250, ErrorMessage = "Reason cannot exceed 250 characters.")]
    public string Reason { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (FromDate.HasValue && ToDate.HasValue && IsSickLeave)
        {
            if (FromDate.Value.Date > DateTime.Today || ToDate.Value.Date > DateTime.Today)
                yield return new ValidationResult("Sick leave can only be requested for today or a past date.", [nameof(FromDate), nameof(ToDate)]);
        }
        else if (FromDate.HasValue && ToDate.HasValue && (FromDate.Value.Date < DateTime.Today || ToDate.Value.Date < DateTime.Today))
        {
            yield return new ValidationResult("Leave can only be applied for today or a future date.", [nameof(FromDate), nameof(ToDate)]);
        }

        if (FromDate.HasValue && ToDate.HasValue && ToDate.Value.Date < FromDate.Value.Date)
            yield return new ValidationResult("To date must be on or after from date.", [nameof(ToDate)]);
    }
}
