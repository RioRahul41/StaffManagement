using System.ComponentModel.DataAnnotations;

namespace StaffManagement.Core.DTOs.ProfileModel;

public sealed class AddressInformationDto
{
    public int EmployeeId { get; set; }
    [Required(ErrorMessage = "Permanent address line 1 is required."), StringLength(200)]
    public string? PermanentAddressLine1 { get; set; }
    [StringLength(200)]
    public string? PermanentAddressLine2 { get; set; }
    [Required(ErrorMessage = "Permanent country is required."), StringLength(100)]
    public string? PermanentCountry { get; set; }
    [Required(ErrorMessage = "Permanent state is required."), StringLength(100)]
    public string? PermanentState { get; set; }
    [Required(ErrorMessage = "Permanent city is required."), StringLength(100)]
    public string? PermanentCity { get; set; }
    [Required(ErrorMessage = "Permanent pin code is required."), StringLength(20)]
    public string? PermanentPincode { get; set; }
    [Required(ErrorMessage = "Current address line 1 is required."), StringLength(200)]
    public string? CurrentAddressLine1 { get; set; }
    [StringLength(200)]
    public string? CurrentAddressLine2 { get; set; }
    [Required(ErrorMessage = "Current country is required."), StringLength(100)]
    public string? CurrentCountry { get; set; }
    [Required(ErrorMessage = "Current state is required."), StringLength(100)]
    public string? CurrentState { get; set; }
    [Required(ErrorMessage = "Current city is required."), StringLength(100)]
    public string? CurrentCity { get; set; }
    [Required(ErrorMessage = "Current pin code is required."), StringLength(20)]
    public string? CurrentPincode { get; set; }
}
