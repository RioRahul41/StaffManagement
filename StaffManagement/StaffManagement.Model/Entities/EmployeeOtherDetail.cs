using System.ComponentModel.DataAnnotations;

namespace StaffManagement.Model.Entities;

public class EmployeeOtherDetail
{
    public int EmployeeOtherDetailId { get; set; }

    public int EmployeeId { get; set; }

    public Employee Employee { get; set; } = null!;

    [Required, StringLength(100)]
    public string DocumentType { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string DocumentNumber { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    public DateTime? ValidFrom { get; set; }

    [DataType(DataType.Date)]
    public DateTime? ValidTo { get; set; }

    [Required, StringLength(260)]
    public string OriginalFileName { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string StoredFileName { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string ContentType { get; set; } = string.Empty;
}
