using System.ComponentModel.DataAnnotations;

namespace StaffManagement.Model.Entities;

public class EmployeeAddressInformation
{
    public int EmployeeAddressInformationId { get; set; }

    public int EmployeeId { get; set; }

    public EmployeeProfile EmployeeProfile { get; set; } = null!;

    [StringLength(200)] public string? PermanentAddressLine1 { get; set; }
    [StringLength(200)] public string? PermanentAddressLine2 { get; set; }
    [StringLength(100)] public string? PermanentCountry { get; set; }
    [StringLength(100)] public string? PermanentState { get; set; }
    [StringLength(100)] public string? PermanentCity { get; set; }
    [StringLength(20)] public string? PermanentPincode { get; set; }

    [StringLength(200)] public string? CurrentAddressLine1 { get; set; }
    [StringLength(200)] public string? CurrentAddressLine2 { get; set; }
    [StringLength(100)] public string? CurrentCountry { get; set; }
    [StringLength(100)] public string? CurrentState { get; set; }
    [StringLength(100)] public string? CurrentCity { get; set; }
    [StringLength(20)] public string? CurrentPincode { get; set; }
}
