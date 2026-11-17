namespace StaffManagement.Core.DTOs.ExpenseModel;

public sealed class EditEmployeeExpenseGroupDto
{
    public int EmployeeId { get; set; }
    public Guid? BatchId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public string LookupTitle { get; set; } = string.Empty;
    public DateTime LookupFromDate { get; set; }
    public DateTime LookupToDate { get; set; }
    public List<EditEmployeeExpenseBillDto> Bills { get; set; } = [];
}

public sealed class EditEmployeeExpenseBillDto
{
    public int Id { get; set; }
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string BillNumber { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? DocumentPath { get; set; }
}
