namespace StaffManagement.Core.DTOs.ProfileModel;

public sealed class OtherDetailsDto
{
    public int EmployeeOtherDetailId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
}
