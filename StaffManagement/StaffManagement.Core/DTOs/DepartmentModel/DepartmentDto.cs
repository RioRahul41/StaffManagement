using System.ComponentModel.DataAnnotations;

namespace StaffManagement.Core.DTOs.DepartmentModel;

public sealed class DepartmentDto
{
    public int DepartmentId { get; set; }

    [Required(ErrorMessage = "Department name is required."), StringLength(100, ErrorMessage = "Department name cannot exceed 100 characters.")]
    public string DepartmentName { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
    public string? Description { get; set; }

    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public bool IsActive { get; set; } = true;
}
