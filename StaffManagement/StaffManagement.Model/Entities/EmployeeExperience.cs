using System.ComponentModel.DataAnnotations;
namespace StaffManagement.Model.Entities;
public sealed class EmployeeExperience : IValidatableObject
{
    public int EmployeeExperienceId { get; set; }
    [Required]
    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    [Required(ErrorMessage = "Previous company is required."), StringLength(200)]
    [Display(Name = "Previous company")]
    public string PreviousCompany { get; set; } = string.Empty;
    [Required(ErrorMessage = "Date of joining is required."), DataType(DataType.Date)]
    [Display(Name = "Date of joining")]
    public DateTime? DateOfJoining { get; set; }
    [Required(ErrorMessage = "Date of relieving is required."), DataType(DataType.Date)]
    [Display(Name = "Date of relieving")]
    public DateTime? DateOfRelieving { get; set; }
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DateOfJoining.HasValue && DateOfRelieving.HasValue && DateOfRelieving.Value.Date < DateOfJoining.Value.Date)
            yield return new ValidationResult("Date of relieving must be on or after the date of joining.", new[] { nameof(DateOfRelieving) });
    }
}
