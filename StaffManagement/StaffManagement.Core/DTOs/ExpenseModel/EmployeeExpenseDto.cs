using System.ComponentModel.DataAnnotations;
using StaffManagement.Model.Enums;

namespace StaffManagement.Core.DTOs.ExpenseModel;

public class EmployeeExpenseDto
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    [Required, StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required, DataType(DataType.Date)]
    public DateTime FromDate { get; set; }

    [Required, DataType(DataType.Date)]
    public DateTime ToDate { get; set; }

    [Range(typeof(decimal), "0.01", "999999999", ErrorMessage = "Amount must be greater than zero.")]
    public decimal Amount { get; set; }

    [StringLength(1000)]
    public string? ApprovalNotes { get; set; }

    public ExpenseStatus Status { get; set; } = ExpenseStatus.Pending;
}
