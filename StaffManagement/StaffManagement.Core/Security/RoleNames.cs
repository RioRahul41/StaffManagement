namespace StaffManagement.Core.Security;

public static class RoleNames
{
    public const string Admin = "Admin";
    public const string Hr = "HR";
    public const string Manager = "Manager";
    public const string User = "User";

    public static readonly string[] All = [Admin, Hr, Manager, User];
}
