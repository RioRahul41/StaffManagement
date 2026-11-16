using System.ComponentModel.DataAnnotations;

namespace StaffManagement.Model.Entities;

public sealed class Holiday
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string HolidayName { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    public DateTime HolidayDate { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
