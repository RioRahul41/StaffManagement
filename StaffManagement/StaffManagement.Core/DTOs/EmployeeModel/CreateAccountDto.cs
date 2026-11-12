using System.ComponentModel.DataAnnotations;

namespace StaffManagement.Core.DTOs.EmployeeModel;

public sealed class CreateAccountDto : EmployeeDto
{
    [Required, StringLength(100, MinimumLength = 8)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required, Compare(nameof(Password))]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = string.Empty;
}
