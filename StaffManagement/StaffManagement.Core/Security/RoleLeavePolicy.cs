namespace StaffManagement.Core.Security;

public readonly record struct RoleLeaveAllowance(int Sick, int Casual);

public static class RoleLeavePolicy
{
    public static RoleLeaveAllowance For(string? role) => role?.Trim().ToUpperInvariant() switch
    {
        "ADMIN" => new(10, 20),
        "HR" => new(8, 15),
        "MANAGER" => new(7, 10),
        "USER" => new(6, 6),
        "TECHNICAL SUPPORT" => new(7, 6),
        _ => new(0, 0)
    };
}
