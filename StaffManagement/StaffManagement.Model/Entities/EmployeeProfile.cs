using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using StaffManagement.Model.Enums;

namespace StaffManagement.Model.Entities;

public class EmployeeProfile
{
    public int EmployeeProfileId { get; set; }

    public int EmployeeId { get; set; }

    public Employee Employee { get; set; } = null!;
    
    public Gender? Gender { get; set; }

    [StringLength(30)]
    public string? MaritalStatus { get; set; }

    [DataType(DataType.Date)]
    public DateTime? DOB { get; set; }

    [StringLength(5)]
    public string? BloodGroup { get; set; }

    [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Phone number must contain exactly 10 digits.")]
    public string? PhoneNumber { get; set; }
    [Required(ErrorMessage ="Enter valid Name")]
    [StringLength(100)]
    public string? EmergencyContactName { get; set; }
    [Required(ErrorMessage ="Must Select Relation")]
    [StringLength(50)]
    public string? EmergencyContactRelationship { get; set; }
    [Required(ErrorMessage ="Must Enter Number")]
    [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Emergency contact phone number must contain exactly 10 digits.")]
    public string? EmergencyContactPhone { get; set; }
    [Required(ErrorMessage = "Permanent address line 1 is required."), StringLength(200)]
    [NotMapped]
    public string? PermanentAddressLine1 { get; set; }

    [StringLength(200)]
    [NotMapped]
    public string? PermanentAddressLine2 { get; set; }

    [Required(ErrorMessage = "Permanent country is required."), StringLength(100)]
    [NotMapped]
    public string? PermanentCountry { get; set; }

    [Required(ErrorMessage = "Permanent state is required."), StringLength(100)]
    [NotMapped]
    public string? PermanentState { get; set; }

    [Required(ErrorMessage = "Permanent city is required."), StringLength(100)]
    [NotMapped]
    public string? PermanentCity { get; set; }

    [Required(ErrorMessage = "Permanent pin code is required."), StringLength(20)]
    [NotMapped]
    public string? PermanentPincode { get; set; }

    [Required(ErrorMessage = "Current address line 1 is required."), StringLength(200)]
    [NotMapped]
    public string? CurrentAddressLine1 { get; set; }

    [StringLength(200)]
    [NotMapped]
    public string? CurrentAddressLine2 { get; set; }

    [Required(ErrorMessage = "Current country is required."), StringLength(100)]
    [NotMapped]
    public string? CurrentCountry { get; set; }

    [Required(ErrorMessage = "Current state is required."), StringLength(100)]
    [NotMapped]
    public string? CurrentState { get; set; }

    [Required(ErrorMessage = "Current city is required."), StringLength(100)]
    [NotMapped]
    public string? CurrentCity { get; set; }

    [Required(ErrorMessage = "Current pin code is required."), StringLength(20)]
    [NotMapped]
    public string? CurrentPincode { get; set; }
}



