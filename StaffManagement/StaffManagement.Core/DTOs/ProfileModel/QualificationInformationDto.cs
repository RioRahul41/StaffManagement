namespace StaffManagement.Core.DTOs.ProfileModel;

public sealed class QualificationInformationDto
{
    public int EmployeeQualificationId { get; set; }
    public string Qualification { get; set; } = string.Empty;
    public string InstitutionName { get; set; } = string.Empty;
    public decimal Percentage { get; set; }
    public int YearOfPassing { get; set; }
}
