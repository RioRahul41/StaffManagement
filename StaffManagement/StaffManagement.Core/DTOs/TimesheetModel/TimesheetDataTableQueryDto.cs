using StaffManagement.Model.Enums;

namespace StaffManagement.Core.DTOs.TimesheetModel;

public sealed class TimesheetDataTableQueryDto
{
    public int Start { get; init; }

    public int Length { get; init; } = 10;

    public string? SearchTerm { get; init; }

    public string? SortColumn { get; init; }

    public bool SortAscending { get; init; }

    public TimesheetStatus? Status { get; init; }

    public DateTime? DateFrom { get; init; }

    public DateTime? DateTo { get; init; }
}
