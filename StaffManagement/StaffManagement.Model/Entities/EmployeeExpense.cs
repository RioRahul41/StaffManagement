using System.ComponentModel.DataAnnotations;
using StaffManagement.Model.Enums;

namespace StaffManagement.Model.Entities;

public class EmployeeExpense
{
    public int Id { get; set; }

    [Required]
    public int EmployeeId { get; set; }

    [Required(ErrorMessage = "First name is required.")]
    [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Title is required.")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "From date is required.")]
    [DataType(DataType.Date)]
    public DateTime FromDate { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "To date is required.")]
    [DataType(DataType.Date)]
    public DateTime ToDate { get; set; } = DateTime.Today;

    [Range(typeof(decimal), "0.01", "999999999", ErrorMessage = "Amount must be greater than zero.")]
    public decimal Amount { get; set; }

    [StringLength(1000, ErrorMessage = "Approval notes cannot exceed 1000 characters.")]
    public string? ApprovalNotes { get; set; }

    public ExpenseStatus Status { get; set; } = ExpenseStatus.Pending;

    public DateTime CreatedTime { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedTime { get; set; }

    public Employee Employee { get; set; } = null!;
}
