using System.ComponentModel.DataAnnotations;

namespace StaffManagement.Core.DTOs.EmployeeModel;

public sealed class EmployeeQualificationDto
{
    public int EmployeeQualificationId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Select a valid employee.")]
    public int EmployeeId { get; set; }

    [Required, StringLength(150)]
    public string Qualification { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string InstitutionName { get; set; } = string.Empty;

    [Range(typeof(decimal), "0", "100", ErrorMessage = "Percentage must be between 0 and 100.")]
    public decimal Percentage { get; set; }

    [Range(1900, 2100, ErrorMessage = "Enter a valid year of passing.")]
    public int YearOfPassing { get; set; }

    [Required, StringLength(260)]
    public string OriginalFileName { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string StoredFileName { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string ContentType { get; set; } = string.Empty;
}
