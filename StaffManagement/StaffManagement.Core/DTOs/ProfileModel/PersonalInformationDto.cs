namespace StaffManagement.Core.DTOs.ProfileModel;

public sealed class PersonalInformationDto
{
    public int EmployeeId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PersonalEmail { get; set; }
    public string? Designation { get; set; }
    public string? Department { get; set; }
    public string? Skills { get; set; }
    public string? ReportingPersonName { get; set; }
    public string? Phone { get; set; }
    public string Location { get; set; } = string.Empty;
    public string? ProfilePhotoPath { get; set; }
}
