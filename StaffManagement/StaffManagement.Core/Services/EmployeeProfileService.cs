using StaffManagement.Core.IRepositories;
using StaffManagement.Core.Iservices;
using StaffManagement.Model.Entities;

namespace StaffManagement.Core.Services;

public sealed class EmployeeProfileService(IEmployeeProfileRepository repository) : IEmployeeProfileService
{
    public Task<IReadOnlyList<EmployeeProfile>> GetAllAsync(CancellationToken cancellationToken = default) =>
        repository.GetAllAsync(cancellationToken);

    public async Task<EmployeeProfile?> GetByEmployeeIdAsync(int employeeId, bool asNoTracking = false, CancellationToken cancellationToken = default)
    {
        var profile = await repository.GetByEmployeeIdAsync(employeeId, asNoTracking, cancellationToken);
        if (profile is null)
            return null;

        var address = await repository.GetAddressInformationByEmployeeIdAsync(employeeId, asNoTracking, cancellationToken);
        CopyAddressToProfile(address, profile);
        return profile;
    }

    public Task<IReadOnlyList<EmployeeOtherDetail>> GetOtherDetailsAsync(int employeeId, CancellationToken cancellationToken = default) =>
        repository.GetOtherDetailsAsync(employeeId, cancellationToken);

    public Task<IReadOnlyList<Employee>> GetActiveEmployeesAsync(CancellationToken cancellationToken = default) =>
        repository.GetActiveEmployeesAsync(cancellationToken);

    public Task<Employee?> GetEmployeeByIdAsync(int employeeId, CancellationToken cancellationToken = default) =>
        repository.GetEmployeeByIdAsync(employeeId, cancellationToken);

    public async Task<int?> GetOwnEmployeeIdAsync(string identityUserId, CancellationToken cancellationToken = default) =>
        (await repository.GetEmployeeByIdentityUserIdAsync(identityUserId, cancellationToken))?.Id;

    public Task<bool> EmployeeHasProfileAsync(int employeeId, CancellationToken cancellationToken = default) =>
        repository.EmployeeHasProfileAsync(employeeId, cancellationToken);

    public Task<bool> IsActiveEmployeeAsync(int employeeId, CancellationToken cancellationToken = default) =>
        repository.IsActiveEmployeeAsync(employeeId, cancellationToken);

    public Task<bool> CanEditAsync(int employeeId, string identityUserId, CancellationToken cancellationToken = default) =>
        repository.EmployeeBelongsToIdentityUserAsync(employeeId, identityUserId, cancellationToken);

    public async Task<(bool Succeeded, string? Error)> CreateAsync(EmployeeProfile profile, CancellationToken cancellationToken = default)
    {
        var employee = await repository.GetEmployeeByIdAsync(profile.EmployeeId, cancellationToken);
        if (employee is null)
            return (false, "Select a valid employee.");
        if (await repository.EmployeeHasProfileAsync(profile.EmployeeId, cancellationToken))
            return (false, "This employee already has profile information. Open it to edit.");

        profile.Gender ??= employee.Gender;
        profile.MaritalStatus ??= employee.MaritalStatus;
        profile.DOB ??= employee.DOB;
        profile.BloodGroup ??= employee.BloodGroup;
        profile.PhoneNumber = employee.Phone;
        CopyProfileFieldsToEmployee(profile, employee);

        await repository.AddAsync(profile, cancellationToken);
        if (HasAnyAddress(profile))
        {
            var address = new EmployeeAddressInformation { EmployeeId = profile.EmployeeId, EmployeeProfile = profile };
            CopyAddressToEntity(profile, address);
            await repository.AddAddressInformationAsync(address, cancellationToken);
        }

        await repository.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    public async Task<bool> UpdateSectionAsync(int employeeId, EmployeeProfile submitted, string section, CancellationToken cancellationToken = default)
    {
        var profile = await repository.GetByEmployeeIdAsync(employeeId, cancellationToken: cancellationToken);
        if (profile is null)
            return false;

        if (section == "emergency")
        {
            profile.EmergencyContactName = submitted.EmergencyContactName;
            profile.EmergencyContactRelationship = submitted.EmergencyContactRelationship;
            profile.EmergencyContactPhone = submitted.EmergencyContactPhone;
            CopyProfileFieldsToEmployee(profile, profile.Employee);
        }
        else if (section == "address")
        {
            var address = await repository.GetAddressInformationByEmployeeIdAsync(employeeId, cancellationToken: cancellationToken);
            if (address is null)
            {
                address = new EmployeeAddressInformation { EmployeeId = employeeId, EmployeeProfile = profile };
                await repository.AddAddressInformationAsync(address, cancellationToken);
            }
            CopyAddressToEntity(submitted, address);
        }
        else if (section == "profile")
        {
            profile.Gender = submitted.Gender;
            profile.MaritalStatus = submitted.MaritalStatus;
            profile.DOB = submitted.DOB;
            profile.BloodGroup = submitted.BloodGroup;
            profile.PhoneNumber = submitted.PhoneNumber;
            CopyProfileFieldsToEmployee(profile, profile.Employee);
        }
        else
        {
            return false;
        }

        await repository.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        var profile = await repository.GetByEmployeeIdAsync(employeeId, cancellationToken: cancellationToken);
        if (profile is null)
            return false;
        repository.Remove(profile);
        await repository.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static bool HasAnyAddress(EmployeeProfile profile) =>
        !string.IsNullOrWhiteSpace(profile.PermanentAddressLine1) ||
        !string.IsNullOrWhiteSpace(profile.PermanentAddressLine2) ||
        !string.IsNullOrWhiteSpace(profile.PermanentCountry) ||
        !string.IsNullOrWhiteSpace(profile.PermanentState) ||
        !string.IsNullOrWhiteSpace(profile.PermanentCity) ||
        !string.IsNullOrWhiteSpace(profile.PermanentPincode) ||
        !string.IsNullOrWhiteSpace(profile.CurrentAddressLine1) ||
        !string.IsNullOrWhiteSpace(profile.CurrentAddressLine2) ||
        !string.IsNullOrWhiteSpace(profile.CurrentCountry) ||
        !string.IsNullOrWhiteSpace(profile.CurrentState) ||
        !string.IsNullOrWhiteSpace(profile.CurrentCity) ||
        !string.IsNullOrWhiteSpace(profile.CurrentPincode);

    private static void CopyAddressToEntity(EmployeeProfile source, EmployeeAddressInformation target)
    {
        target.PermanentAddressLine1 = source.PermanentAddressLine1;
        target.PermanentAddressLine2 = source.PermanentAddressLine2;
        target.PermanentCountry = source.PermanentCountry;
        target.PermanentState = source.PermanentState;
        target.PermanentCity = source.PermanentCity;
        target.PermanentPincode = source.PermanentPincode;
        target.CurrentAddressLine1 = source.CurrentAddressLine1;
        target.CurrentAddressLine2 = source.CurrentAddressLine2;
        target.CurrentCountry = source.CurrentCountry;
        target.CurrentState = source.CurrentState;
        target.CurrentCity = source.CurrentCity;
        target.CurrentPincode = source.CurrentPincode;
    }

    private static void CopyAddressToProfile(EmployeeAddressInformation? source, EmployeeProfile target)
    {
        target.PermanentAddressLine1 = source?.PermanentAddressLine1;
        target.PermanentAddressLine2 = source?.PermanentAddressLine2;
        target.PermanentCountry = source?.PermanentCountry;
        target.PermanentState = source?.PermanentState;
        target.PermanentCity = source?.PermanentCity;
        target.PermanentPincode = source?.PermanentPincode;
        target.CurrentAddressLine1 = source?.CurrentAddressLine1;
        target.CurrentAddressLine2 = source?.CurrentAddressLine2;
        target.CurrentCountry = source?.CurrentCountry;
        target.CurrentState = source?.CurrentState;
        target.CurrentCity = source?.CurrentCity;
        target.CurrentPincode = source?.CurrentPincode;
    }

    private static void CopyProfileFieldsToEmployee(EmployeeProfile profile, Employee employee)
    {
        employee.Gender = profile.Gender;
        employee.MaritalStatus = profile.MaritalStatus;
        employee.DOB = profile.DOB;
        employee.BloodGroup = profile.BloodGroup;
        employee.Phone = profile.PhoneNumber;
    }
}
