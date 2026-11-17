using System.ComponentModel.DataAnnotations;
using StaffManagement.Model.Enums;

namespace StaffManagement.Core.DTOs.ExpenseModel;

public class CreateEmployeeExpenseDto
{
    [Required(ErrorMessage = "Employee is required.")]
    public int? EmployeeId { get; set; }

    [Required(ErrorMessage = "Title is required."), StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "From date is required."), DataType(DataType.Date)]
    public DateTime? FromDate { get; set; }

    [Required(ErrorMessage = "To date is required."), DataType(DataType.Date)]
    public DateTime? ToDate { get; set; }

    [Required(ErrorMessage = "Category is required."), StringLength(100)]
    public string Category { get; set; } = string.Empty;

    [Required(ErrorMessage = "Amount is required."), Range(typeof(decimal), "0.01", "999999999", ErrorMessage = "Amount must be greater than zero.")]
    public decimal? Amount { get; set; }

    [Required(ErrorMessage = "Bill number is required."), StringLength(100)]
    public string BillNumber { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [Required, Range(typeof(decimal), "0.01", "999999999", ErrorMessage = "Total amount must be greater than zero.")]
    public decimal? BillAmount { get; set; }

    [StringLength(1000)]
    public string? ApprovalNotes { get; set; }

    public ExpenseStatus Status { get; set; } = ExpenseStatus.Pending;
}
