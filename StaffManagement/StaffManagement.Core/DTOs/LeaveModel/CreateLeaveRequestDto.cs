using System.ComponentModel.DataAnnotations;
using StaffManagement.Model.Enums;

namespace StaffManagement.Core.DTOs.LeaveModel;

public class CreateLeaveRequestDto : IValidatableObject
{
    [EnumDataType(typeof(LeaveType))]
    public LeaveType LeaveType { get; set; }

    [Required, DataType(DataType.Date)]
    public DateTime FromDate { get; set; } = DateTime.Today;

    [Required, DataType(DataType.Date)]
    public DateTime ToDate { get; set; } = DateTime.Today;

    [Required, StringLength(250, ErrorMessage = "Reason cannot exceed 250 characters.")]
    public string Reason { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (FromDate.Date < DateTime.Today || ToDate.Date < DateTime.Today)
            yield return new ValidationResult("Leave can only be applied for today or a future date.", [nameof(FromDate), nameof(ToDate)]);

        if (ToDate.Date < FromDate.Date)
            yield return new ValidationResult("To date must be on or after from date.", [nameof(ToDate)]);
    }
}
