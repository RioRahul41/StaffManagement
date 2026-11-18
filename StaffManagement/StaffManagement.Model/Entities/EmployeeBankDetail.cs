using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
namespace StaffManagement.Model.Entities;
public sealed class EmployeeBankDetail
{
    public int EmployeeBankDetailId { get; set; }
    [Required]
    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    [Required(ErrorMessage = "Account holder name is required."), StringLength(150)]
    [Display(Name = "Account holder name")]
    public string AccountHolderName { get; set; } = string.Empty;
    [Required(ErrorMessage = "Bank name is required."), StringLength(150)]
    [Display(Name = "Bank name")]
    public string AccountName { get; set; } = string.Empty;
    [Required(ErrorMessage = "IFSC code is required."), StringLength(11, MinimumLength = 11)]
    [RegularExpression(@"^[A-Z]{4}0[A-Z0-9]{6}$", ErrorMessage = "Enter a valid 11-character IFSC code.")]
    [Display(Name = "IFSC code")]
    public string IFSCCode { get; set; } = string.Empty;
    [Required(ErrorMessage = "Branch name is required."), StringLength(150)]
    [Display(Name = "Branch name")]
    public string Branch { get; set; } = string.Empty;
    [Required(ErrorMessage = "Account number is required."), StringLength(34, MinimumLength = 6)]
    [RegularExpression(@"^[0-9]{6,34}$", ErrorMessage = "Account number must contain 6 to 34 digits.")]
    [Display(Name = "Account number")]
    public string AccountNumber { get; set; } = string.Empty;
}


