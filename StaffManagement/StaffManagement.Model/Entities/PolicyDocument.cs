using System.ComponentModel.DataAnnotations;

namespace StaffManagement.Model.Entities;

public sealed class PolicyDocument
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string PolicyName { get; set; } = string.Empty;

    [Required, StringLength(255)]
    public string OriginalFileName { get; set; } = string.Empty;

    [Required, StringLength(80)]
    public string StoredFileName { get; set; } = string.Empty;

    [Required, StringLength(256)]
    public string UploadedBy { get; set; } = string.Empty;

    public DateTime UploadedAtUtc { get; set; } = DateTime.UtcNow;
}
