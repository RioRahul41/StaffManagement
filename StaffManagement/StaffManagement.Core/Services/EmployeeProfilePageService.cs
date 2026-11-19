using StaffManagement.Core.DTOs.EmployeeModel;
using StaffManagement.Core.DTOs.ProfileModel;
using StaffManagement.Core.Iservices;

namespace StaffManagement.Core.Services;

public sealed class EmployeeProfilePageService(
    IEmployeeProfileService profileService,
    IEmployeeQualificationService qualificationService,
    IEmployeeExperienceService experienceService,
    IEmployeeBankDetailService bankDetailService) : IEmployeeProfilePageService
{
    public async Task<EmployeeProfilePageDto> BuildAsync(EmployeeDto employee, CancellationToken cancellationToken = default)
    {
        var page = CopyEmployeeFields(employee);
        page.PersonalInformation = new PersonalInformationDto
        {
            EmployeeId = employee.Id,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Email = employee.Email,
            PersonalEmail = employee.PersonalEmail,
            Designation = employee.Designation,
            Department = employee.Department,
            Skills = employee.Skills,
            ReportingPersonName = employee.ReportingPersonName,
            Phone = employee.Phone,
            Location = employee.Location,
            ProfilePhotoPath = employee.ProfilePhotoPath
        };

        var profile = await profileService.GetByEmployeeIdAsync(employee.Id, asNoTracking: true, cancellationToken);
        if (profile is not null)
        {
            page.ProfileInformation = new ProfileInformationDto
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
            };
            page.AddressInformation = new AddressInformationDto
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
            };
        }

        var otherDetails = await profileService.GetOtherDetailsAsync(employee.Id, cancellationToken);
        page.OtherDetails = otherDetails.Select(item => new OtherDetailsDto
        {
            EmployeeOtherDetailId = item.EmployeeOtherDetailId,
            DocumentType = item.DocumentType,
            DocumentNumber = item.DocumentNumber,
            ValidFrom = item.ValidFrom,
            ValidTo = item.ValidTo
        }).ToList();

        var qualifications = await qualificationService.GetByEmployeeIdAsync(employee.Id, cancellationToken);
        page.Qualifications = qualifications.Select(item => new QualificationInformationDto
        {
            EmployeeQualificationId = item.EmployeeQualificationId,
            Qualification = item.Qualification,
            InstitutionName = item.InstitutionName,
            Percentage = item.Percentage,
            YearOfPassing = item.YearOfPassing
        }).ToList();

        var experiences = await experienceService.GetByEmployeeIdAsync(employee.Id, cancellationToken);
        page.Experiences = experiences.Select(item => new ExperienceInformationDto
        {
            EmployeeExperienceId = item.EmployeeExperienceId,
            PreviousCompany = item.PreviousCompany,
            DateOfJoining = item.DateOfJoining,
            DateOfRelieving = item.DateOfRelieving
        }).ToList();

        page.BankDetails = await bankDetailService.GetByEmployeeIdAsync(employee.Id, cancellationToken);

        return page;
    }

    private static EmployeeProfilePageDto CopyEmployeeFields(EmployeeDto source) => new()
    {
        Id = source.Id,
        IdentityUserId = source.IdentityUserId,
        ReportingPersonId = source.ReportingPersonId,
        ReportingPersonName = source.ReportingPersonName,
        FirstName = source.FirstName,
        LastName = source.LastName,
        Email = source.Email,
        PersonalEmail = source.PersonalEmail,
        Skills = source.Skills,
        ProfilePhotoPath = source.ProfilePhotoPath,
        Phone = source.Phone,
        DepartmentId = source.DepartmentId,
        ProjectIds = source.ProjectIds,
        AssignedProjects = source.AssignedProjects,
        Department = source.Department,
        Location = source.Location,
        CreatedTime = source.CreatedTime,
        DOB = source.DOB,
        Salary = source.Salary,
        Designation = source.Designation,
        DesignationId = source.DesignationId,
        Gender = source.Gender,
        MaritalStatus = source.MaritalStatus,
        BloodGroup = source.BloodGroup,
        EmergencyContactName = source.EmergencyContactName,
        EmergencyContactRelationship = source.EmergencyContactRelationship,
        EmergencyContactPhone = source.EmergencyContactPhone,
        EmpIsActive = source.EmpIsActive,
        SickLeaveAllowance = source.SickLeaveAllowance,
        CasualLeaveAllowance = source.CasualLeaveAllowance
    };
}
