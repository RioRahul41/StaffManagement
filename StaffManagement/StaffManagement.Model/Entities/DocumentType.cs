using System.ComponentModel.DataAnnotations;

namespace StaffManagement.Model.Entities;

public sealed class DocumentType
{
    public int DocumentTypeId { get; set; }

    [Required, StringLength(100)]
    public string DocumentTypeName { get; set; } = string.Empty;

    public bool RequiresValidityDates { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
    public ICollection<EmployeeOtherDetail> EmployeeOtherDetails { get; set; } = new List<EmployeeOtherDetail>();
}
