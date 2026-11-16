using System.ComponentModel.DataAnnotations;

namespace StaffManagement.Model.Entities;

public sealed class LeaveTypeMaster
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedDate { get; set; }
}
