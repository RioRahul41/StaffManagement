using System.ComponentModel.DataAnnotations;

namespace StaffManagement.Core.DTOs.DesignationModel;

public sealed class DesignationDto
{
    public int DesignationId { get; set; }

    [Required(ErrorMessage = "Designation name is required."), StringLength(100, ErrorMessage = "Designation name cannot exceed 100 characters.")]
    public string DesignationName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Select a department.")]
    public int? DepartmentId { get; set; }

    public string? DepartmentName { get; set; }

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
    public string? Description { get; set; }

    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public bool IsActive { get; set; } = true;
}
