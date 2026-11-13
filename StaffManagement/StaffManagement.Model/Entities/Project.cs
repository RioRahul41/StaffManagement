using System.ComponentModel.DataAnnotations;

namespace StaffManagement.Model.Entities;

public class Project
{
    [Key]
    public int ProjectId { get; set; }

    [Required(ErrorMessage = "Project name is required.")]
    [StringLength(150, ErrorMessage = "Project name cannot exceed 150 characters.")]
    [Display(Name = "Project name")]
    public string ProjectName { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
    public string? Description { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Start date")]
    public DateTime? StartDate { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "End date")]
    public DateTime? EndDate { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedDate { get; set; }

    public bool IsActive { get; set; } = true;

}
