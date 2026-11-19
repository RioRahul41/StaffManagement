using StaffManagement.Core.IRepositories;
using StaffManagement.Core.Iservices;
using StaffManagement.Core.DTOs.ProfileModel;
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

    public async Task<EmployeeProfileDto?> GetFormByEmployeeIdAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        var profile = await GetByEmployeeIdAsync(employeeId, asNoTracking: true, cancellationToken);
        if (profile is null)
            return null;

        return new EmployeeProfileDto
        {
            EmployeeId = profile.EmployeeId,
            EmployeeName = profile.Employee is null ? null : $"{profile.Employee.FirstName} {profile.Employee.LastName}",
            ProfileInformation = new ProfileInformationDto
            {
                EmployeeId = profile.EmployeeId,
                Gender = profile.Gender,
                MaritalStatus = profile.MaritalStatus,
                DOB = profile.DOB,
                BloodGroup = profile.BloodGroup,
                PhoneNumber = profile.PhoneNumber,
                EmergencyContactName = profile.EmergencyContactName,
                EmergencyContactRelationship = profile.EmergencyContactRelationship,
                EmergencyContactPhone = profile.EmergencyContactPhone
            },
            AddressInformation = new AddressInformationDto
            {
                EmployeeId = profile.EmployeeId,
                PermanentAddressLine1 = profile.PermanentAddressLine1,
                PermanentAddressLine2 = profile.PermanentAddressLine2,
                PermanentCountry = profile.PermanentCountry,
                PermanentState = profile.PermanentState,
                PermanentCity = profile.PermanentCity,
                PermanentPincode = profile.PermanentPincode,
                CurrentAddressLine1 = profile.CurrentAddressLine1,
                CurrentAddressLine2 = profile.CurrentAddressLine2,
                CurrentCountry = profile.CurrentCountry,
                CurrentState = profile.CurrentState,
                CurrentCity = profile.CurrentCity,
                CurrentPincode = profile.CurrentPincode
            }
        };
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

    public async Task<(bool Succeeded, string? Error)> CreateAsync(EmployeeProfileDto model, CancellationToken cancellationToken = default)
    {
        var employee = await repository.GetEmployeeByIdAsync(model.EmployeeId, cancellationToken);
        if (employee is null)
            return (false, "Select a valid employee.");
        if (await repository.EmployeeHasProfileAsync(model.EmployeeId, cancellationToken))
            return (false, "This employee already has profile information. Open it to edit.");

        var profile = ToEntity(model);
        profile.Gender ??= employee.Gender;
        profile.MaritalStatus ??= employee.MaritalStatus;
        profile.DOB ??= employee.DOB?.Date;
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

    public async Task<bool> UpdateSectionAsync(int employeeId, EmployeeProfileDto submitted, string section, CancellationToken cancellationToken = default)
    {
        var profile = await repository.GetByEmployeeIdAsync(employeeId, cancellationToken: cancellationToken);
        if (profile is null)
            return false;

        if (section == "emergency")
        {
            profile.EmergencyContactName = submitted.ProfileInformation.EmergencyContactName;
            profile.EmergencyContactRelationship = submitted.ProfileInformation.EmergencyContactRelationship;
            profile.EmergencyContactPhone = submitted.ProfileInformation.EmergencyContactPhone;
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
            CopyAddressToEntity(submitted.AddressInformation, address);
        }
        else if (section == "profile")
        {
            profile.Gender = submitted.ProfileInformation.Gender;
            profile.MaritalStatus = submitted.ProfileInformation.MaritalStatus;
            profile.DOB = submitted.ProfileInformation.DOB?.Date;
            profile.BloodGroup = submitted.ProfileInformation.BloodGroup;
            profile.PhoneNumber = submitted.ProfileInformation.PhoneNumber;
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

    private static void CopyAddressToEntity(AddressInformationDto source, EmployeeAddressInformation target)
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
        employee.DOB = profile.DOB?.Date;
        employee.BloodGroup = profile.BloodGroup;
        employee.Phone = profile.PhoneNumber;
    }

    private static EmployeeProfile ToEntity(EmployeeProfileDto source) => new()
    {
        EmployeeId = source.EmployeeId,
        Gender = source.ProfileInformation.Gender,
        MaritalStatus = source.ProfileInformation.MaritalStatus,
        DOB = source.ProfileInformation.DOB?.Date,
        BloodGroup = source.ProfileInformation.BloodGroup,
        PhoneNumber = source.ProfileInformation.PhoneNumber,
        EmergencyContactName = source.ProfileInformation.EmergencyContactName,
        EmergencyContactRelationship = source.ProfileInformation.EmergencyContactRelationship,
        EmergencyContactPhone = source.ProfileInformation.EmergencyContactPhone,
        PermanentAddressLine1 = source.AddressInformation.PermanentAddressLine1,
        PermanentAddressLine2 = source.AddressInformation.PermanentAddressLine2,
        PermanentCountry = source.AddressInformation.PermanentCountry,
        PermanentState = source.AddressInformation.PermanentState,
        PermanentCity = source.AddressInformation.PermanentCity,
        PermanentPincode = source.AddressInformation.PermanentPincode,
        CurrentAddressLine1 = source.AddressInformation.CurrentAddressLine1,
        CurrentAddressLine2 = source.AddressInformation.CurrentAddressLine2,
        CurrentCountry = source.AddressInformation.CurrentCountry,
        CurrentState = source.AddressInformation.CurrentState,
        CurrentCity = source.AddressInformation.CurrentCity,
        CurrentPincode = source.AddressInformation.CurrentPincode
    };
}
