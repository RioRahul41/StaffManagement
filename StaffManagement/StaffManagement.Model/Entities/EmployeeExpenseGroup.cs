using StaffManagement.Model.Enums;

namespace StaffManagement.Model.Entities;

public sealed class EmployeeExpenseGroup
{
    public int EmployeeId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public decimal BillAmount { get; set; }
    public ExpenseStatus Status { get; set; }
    public string? ApprovalNotes { get; set; }
    public List<EmployeeExpense> Expenses { get; set; } = [];
}
