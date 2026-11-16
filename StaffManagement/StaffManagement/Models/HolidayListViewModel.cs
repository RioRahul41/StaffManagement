using System.ComponentModel.DataAnnotations;
using StaffManagement.Model.Entities;

namespace StaffManagement.Models;

public sealed class HolidayListViewModel
{
    public IReadOnlyList<Holiday> Holidays { get; init; } = [];

    [Required, StringLength(120)]
    public string HolidayName { get; set; } = string.Empty;

    [Required, DataType(DataType.Date)]
    public DateTime? HolidayDate { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }
}
