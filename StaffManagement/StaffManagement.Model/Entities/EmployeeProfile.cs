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

    [StringLength(100)]
    public string? EmergencyContactName { get; set; }

    [StringLength(50)]
    public string? EmergencyContactRelationship { get; set; }

    [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Emergency contact phone number must contain exactly 10 digits.")]
    public string? EmergencyContactPhone { get; set; }
    [StringLength(200)]
    [NotMapped]
    public string? PermanentAddressLine1 { get; set; }

    [StringLength(200)]
    [NotMapped]
    public string? PermanentAddressLine2 { get; set; }

    [StringLength(100)]
    [NotMapped]
    public string? PermanentCountry { get; set; }

    [StringLength(100)]
    [NotMapped]
    public string? PermanentState { get; set; }

    [StringLength(100)]
    [NotMapped]
    public string? PermanentCity { get; set; }

    [StringLength(20)]
    [NotMapped]
    public string? PermanentPincode { get; set; }

    [StringLength(200)]
    [NotMapped]
    public string? CurrentAddressLine1 { get; set; }

    [StringLength(200)]
    [NotMapped]
    public string? CurrentAddressLine2 { get; set; }

    [StringLength(100)]
    [NotMapped]
    public string? CurrentCountry { get; set; }

    [StringLength(100)]
    [NotMapped]
    public string? CurrentState { get; set; }

    [StringLength(100)]
    [NotMapped]
    public string? CurrentCity { get; set; }

    [StringLength(20)]
    [NotMapped]
    public string? CurrentPincode { get; set; }
}



