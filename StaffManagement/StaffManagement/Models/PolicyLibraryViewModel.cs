using StaffManagement.Model.Entities;

namespace StaffManagement.Models;

public sealed class PolicyLibraryViewModel
{
    public IReadOnlyList<PolicyDocument> Documents { get; init; } = [];
    public string PolicyName { get; init; } = string.Empty;
}
