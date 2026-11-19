using StaffManagement.Core.DTOs.EmployeeModel;

namespace StaffManagement.Core.DTOs.ProfileModel;

// EmployeeDto base fields keep the existing self-edit form field names and validation binding.
public sealed class EmployeeProfilePageDto : EmployeeDto
{
    public PersonalInformationDto PersonalInformation { get; set; } = new();
    public ProfileInformationDto? ProfileInformation { get; set; }
    public AddressInformationDto AddressInformation { get; set; } = new();
    public IReadOnlyList<OtherDetailsDto> OtherDetails { get; set; } = [];
    public IReadOnlyList<QualificationInformationDto> Qualifications { get; set; } = [];
    public IReadOnlyList<ExperienceInformationDto> Experiences { get; set; } = [];
    public BankDetailsDto? BankDetails { get; set; }
}
