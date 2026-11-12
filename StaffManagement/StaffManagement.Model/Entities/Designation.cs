using System.ComponentModel.DataAnnotations;

namespace StaffManagement.Model.Entities;

public class Designation
{
    [Key]
    public int DesignationId { get; set; }

    [Required(ErrorMessage = "Designation name is required.")]
    [StringLength(100, ErrorMessage = "Designation name cannot exceed 100 characters.")]
    public string DesignationName { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
    public string? Description { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedDate { get; set; }

    public bool IsActive { get; set; } = true;
}
