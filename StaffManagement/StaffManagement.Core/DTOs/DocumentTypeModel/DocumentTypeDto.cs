using System.ComponentModel.DataAnnotations;

namespace StaffManagement.Core.DTOs.DocumentTypeModel;

public sealed class DocumentTypeDto
{
    public int DocumentTypeId { get; set; }

    [Required(ErrorMessage = "Document type name is required."), StringLength(100)]
    [Display(Name = "Document type")]
    public string DocumentTypeName { get; set; } = string.Empty;

    [Display(Name = "Validity dates are required")]
    public bool RequiresValidityDates { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}
