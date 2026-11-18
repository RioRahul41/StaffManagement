using System.ComponentModel.DataAnnotations;

namespace StaffManagement.Core.DTOs.SkillModel;

public sealed class SkillCreateViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Skill name is required.")]
    [StringLength(100, ErrorMessage = "Skill name cannot exceed 100 characters.")]
    public string SkillName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
