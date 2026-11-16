using StaffManagement.Model.Enums;

namespace StaffManagement.Core.DTOs.ExpenseModel;

public sealed class EmployeeExpenseGroupDto
{
    public int EmployeeId { get; init; }
    public Guid? BatchId { get; init; }
    public string Title { get; init; } = string.Empty;
    public DateTime FromDate { get; init; }
    public DateTime ToDate { get; init; }
    public IReadOnlyList<string> Categories { get; init; } = [];
    public decimal BillAmount { get; init; }
    public string? ApprovalNotes { get; init; }
    public string Status { get; init; } = string.Empty;
    public IReadOnlyList<EmployeeExpenseDto> Expenses { get; init; } = [];
}
