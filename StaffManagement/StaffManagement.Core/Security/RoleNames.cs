namespace StaffManagement.Core.Security;

public static class RoleNames
{
    public const string Admin = "Admin";
    public const string Hr = "HR";
    public const string Manager = "Manager";
    public const string User = "User";
    public const string TechnicalSupport = "Technical Support";

    public static readonly string[] All = [Admin, Hr, Manager, User, TechnicalSupport];

    public static IReadOnlyList<string> ReportingRolesFor(string? employeeRole) => employeeRole?.ToUpperInvariant() switch
    {
        "HR" => [Admin],
        "MANAGER" => [Admin],
        "USER" => [Hr, Manager],
        "TECHNICAL SUPPORT" => [Admin],
        _ => []
    };

    public static bool RequiresReportingPerson(string? employeeRole) =>
        string.Equals(employeeRole, Hr, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(employeeRole, Manager, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(employeeRole, User, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(employeeRole, TechnicalSupport, StringComparison.OrdinalIgnoreCase);
}
