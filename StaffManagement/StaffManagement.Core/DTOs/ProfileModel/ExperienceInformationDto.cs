using System.ComponentModel.DataAnnotations;

namespace StaffManagement.Core.DTOs.ProfileModel;

public sealed class ExperienceInformationDto : IValidatableObject
{
    public int EmployeeExperienceId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Select a valid employee.")]
    public int EmployeeId { get; set; }

    [Required(ErrorMessage = "Previous company is required."), StringLength(200)]
    public string PreviousCompany { get; set; } = string.Empty;

    [Required(ErrorMessage = "Date of joining is required."), DataType(DataType.Date)]
    public DateTime? DateOfJoining { get; set; }

    [Required(ErrorMessage = "Date of relieving is required."), DataType(DataType.Date)]
    public DateTime? DateOfRelieving { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DateOfJoining.HasValue && DateOfRelieving.HasValue && DateOfRelieving.Value.Date < DateOfJoining.Value.Date)
            yield return new ValidationResult("Date of relieving must be on or after date of joining.", [nameof(DateOfRelieving)]);
    }
}
