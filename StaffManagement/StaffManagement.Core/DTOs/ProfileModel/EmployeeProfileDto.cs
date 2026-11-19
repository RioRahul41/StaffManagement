using System.ComponentModel.DataAnnotations;

namespace StaffManagement.Core.DTOs.ProfileModel;

public sealed class EmployeeProfileDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Select a valid employee.")]
    public int EmployeeId { get; set; }

    public string? EmployeeName { get; set; }
    public ProfileInformationDto ProfileInformation { get; set; } = new();
    public AddressInformationDto AddressInformation { get; set; } = new();
}
