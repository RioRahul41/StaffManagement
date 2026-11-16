using StaffManagement.Core.DTOs.ExpenseModel;

namespace StaffManagement.Models;

public sealed class CreateExpenseBatchViewModel
{
    public List<CreateEmployeeExpenseDto> Expenses { get; set; } = [new()];
}
