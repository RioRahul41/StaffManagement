using System.ComponentModel.DataAnnotations;
using StaffManagement.Model.Enums;

namespace StaffManagement.Core.DTOs.ProfileModel;

public sealed class ProfileInformationDto
{
    public int EmployeeId { get; set; }
    public Gender? Gender { get; set; }
    [StringLength(30)]
    public string? MaritalStatus { get; set; }
    public DateTime? DOB { get; set; }
    [StringLength(5)]
    public string? BloodGroup { get; set; }
    [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Phone number must contain exactly 10 digits.")]
    public string? PhoneNumber { get; set; }
    [Required(ErrorMessage = "Enter valid Name"), StringLength(100)]
    public string? EmergencyContactName { get; set; }
    [Required(ErrorMessage = "Must Select Relation"), StringLength(50)]
    public string? EmergencyContactRelationship { get; set; }
    [Required(ErrorMessage = "Must Enter Number"), RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Emergency contact phone number must contain exactly 10 digits.")]
    public string? EmergencyContactPhone { get; set; }
}
