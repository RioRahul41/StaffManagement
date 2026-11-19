using StaffManagement.Core.DTOs.EmployeeModel;
using StaffManagement.Core.DTOs.ProfileModel;

namespace StaffManagement.Core.Iservices;

public interface IEmployeeProfilePageService
{
    Task<EmployeeProfilePageDto> BuildAsync(EmployeeDto employee, CancellationToken cancellationToken = default);
}
