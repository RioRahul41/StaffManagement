using StaffManagement.Model.Enums;

namespace StaffManagement.Model.Entities;

public sealed class EmployeeCelebrationNotification
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public EmployeeCelebrationType CelebrationType { get; set; }
    public DateTime CelebrationDate { get; set; }
    public DateTime ShownAtUtc { get; set; } = DateTime.UtcNow;

    public Employee Employee { get; set; } = null!;
}
