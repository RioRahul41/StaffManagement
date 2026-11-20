using Microsoft.AspNetCore.Identity;
using StaffManagement.Core.DTOs.EmployeeModel;
using StaffManagement.Core.IRepositories;
using StaffManagement.Core.Iservices;
using StaffManagement.Model.Entities;

namespace StaffManagement.Core.Services
{
    public sealed class EmployeeService(
        IEmployeeRepository repository,
        IProjectRepository projectRepository,
        ILeaveRequestRepository leaveRequestRepository,
         IRelievingTypeRepository relievingTypeRepository,
        UserManager<ApplicationIdentity> userManager) : IEmployeeService
    {
        public async Task<IReadOnlyList<EmployeeDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
            (await repository.GetAllAsync(cancellationToken)).Select(Map).ToList();

        public async Task<(IReadOnlyList<EmployeeDto> Items, int TotalCount)> GetPageAsync(
            string? searchTerm,
            int? departmentId,
            string? designation,
            bool? isActive,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var result = await repository.GetPageAsync(
                searchTerm, departmentId, designation, isActive, page, pageSize, cancellationToken);
            return (result.Items.Select(Map).ToList(), result.TotalCount);
        }

        public async Task<(IReadOnlyList<EmployeeDto> Items, int TotalCount, int FilteredCount)> GetDataTablePageAsync(
            EmployeeDataTableQueryDto query,
            CancellationToken cancellationToken = default)
        {
            var normalizedQuery = new EmployeeDataTableQueryDto
            {
                Start = Math.Max(query.Start, 0),
                Length = Math.Clamp(query.Length, 1, 100),
                SearchTerm = query.SearchTerm?.Trim(),
                SortColumn = query.SortColumn,
                SortAscending = query.SortAscending,
                DepartmentId = query.DepartmentId,
                Designation = query.Designation,
                IsActive = query.IsActive
            };

            var result = await repository.GetDataTablePageAsync(normalizedQuery, cancellationToken);
            return (result.Items.Select(Map).ToList(), result.TotalCount, result.FilteredCount);
        }

        public async Task<EmployeeDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var employee = await repository.GetByIdAsync(id, cancellationToken);
            return employee is null ? null : Map(employee);
        }

        public async Task<EmployeeDto?> GetByIdentityUserIdAsync(string identityUserId, CancellationToken cancellationToken = default)
        {
            var employee = await repository.GetByIdentityUserIdAsync(identityUserId, cancellationToken);
            return employee is null ? null : Map(employee);
        }

        public async Task<(bool Succeeded, string? Error)> CreateAsync(
            EmployeeDto model,
            string identityUserId,
            CancellationToken cancellationToken = default)
        {
            if (await repository.EmailExistsAsync(model.Email, cancellationToken: cancellationToken))
                return (false, "An employee with this email already exists.");

            var employee = Map(model);
            employee.IdentityUserId = identityUserId;
            employee.CreatedTime = DateTime.UtcNow;
            employee.EmpIsActive = true;

            await repository.AddAsync(employee, cancellationToken);
            await repository.SaveChangesAsync(cancellationToken);
            return (true, null);
        }

        public async Task<(bool Succeeded, string? Error)> UpdateAsync(EmployeeDto model, CancellationToken cancellationToken = default)
        {
            var employee = await repository.GetByIdAsync(model.Id, cancellationToken);
            if (employee is null)
                return (false, "Employee was not found.");

            if (await repository.EmailExistsAsync(model.Email, model.Id, cancellationToken))
                return (false, "An employee with this email already exists.");

            var projectIds = model.ProjectIds.Distinct().ToArray();
            var projects = await projectRepository.GetActiveByIdsAsync(projectIds);
            if (projects.Count != projectIds.Length)
                return (false, "One or more selected projects are no longer active.");

            var leaveTotals = await leaveRequestRepository.GetLeaveTotalsAsync(employee.Id, cancellationToken);
            employee.FirstName = model.FirstName;
            employee.LastName = model.LastName;
            employee.Email = model.Email;
            employee.PersonalEmail = model.PersonalEmail;
            employee.Skills = model.Skills;
            employee.ProfilePhotoPath = model.ProfilePhotoPath;
            employee.Phone = model.Phone;
            employee.DepartmentId = model.DepartmentId;
            employee.ReportingPersonId = model.ReportingPersonId;
            employee.ProjectAssignments.Clear();
            foreach (var project in projects)
            {
                employee.ProjectAssignments.Add(new EmployeeProject
                {
                    EmployeeId = employee.Id,
                    ProjectId = project.ProjectId
                });
            }
            employee.Department = model.Department;
            employee.Location = model.Location;
            employee.DOB = model.DOB?.Date;
            employee.Salary = model.Salary;
            employee.Designation = model.Designation;
            employee.DesignationId = model.DesignationId;
            employee.Gender = model.Gender;
            employee.MaritalStatus = model.MaritalStatus;
            employee.BloodGroup = model.BloodGroup;
            employee.EmpIsActive = model.EmpIsActive;
            employee.SickLeaveAllowance = model.SickLeaveAllowance;
            employee.SickLeaveRemaining = Math.Max(0, model.SickLeaveAllowance - leaveTotals.SickUsed - leaveTotals.SickPending);
            employee.CasualLeaveAllowance = model.CasualLeaveAllowance;
            employee.CasualLeaveRemaining = Math.Max(0, model.CasualLeaveAllowance - leaveTotals.CasualUsed - leaveTotals.CasualPending);
            employee.UpdatedTime = DateTime.UtcNow;

            repository.Update(employee);
            await repository.SaveChangesAsync(cancellationToken);
            return (true, null);
        }


        public async Task<(bool Succeeded, string? Error)> DeleteAsync(
            int id,
            int? relievingTypeId,
            string? deletedReason,
            string deletedBy,
            CancellationToken cancellationToken = default)
        {
            var employee = await repository.GetByIdAsync(id, cancellationToken);

            if (employee is null)
                return (false, "Employee was not found.");

            if (!relievingTypeId.HasValue)
                return (false, "Please select a relieving type.");

            var relievingType = await relievingTypeRepository.GetByIdAsync(
                relievingTypeId.Value);

            if (relievingType is null || !relievingType.IsActive)
                return (false, "Selected relieving type is unavailable.");

            employee.RelievingTypeId = relievingTypeId.Value;
            employee.DeletedReason = deletedReason?.Trim();
            employee.DeletedDate = DateTime.UtcNow;
            employee.DeletedBy = deletedBy;

            // Soft delete: do not permanently remove the employee
            employee.EmpIsDeleted = true;
            employee.EmpIsActive = false;
         var identityuser = await userManager.FindByIdAsync(employee.IdentityUserId);
            if(identityuser!= null)
            {
                identityuser.UserIsActive = false;
                identityuser.UserIsDeleted = true;
                await userManager.UpdateAsync(identityuser);
            }

            repository.Update(employee);

            await repository.SaveChangesAsync(cancellationToken);

            return (true, null);
        }


        private static EmployeeDto Map(Employee employee) => new()
        {
            Id = employee.Id,
            IdentityUserId = employee.IdentityUserId,
            ReportingPersonId = employee.ReportingPersonId,
            ReportingPersonName = employee.ReportingPerson is null
                ? null
                : $"{employee.ReportingPerson.FirstName} {employee.ReportingPerson.LastName}",
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Email = employee.Email,
            PersonalEmail = employee.PersonalEmail,
            Skills = employee.Skills,
            ProfilePhotoPath = employee.ProfilePhotoPath,
            Phone = employee.Phone,
            DepartmentId = employee.DepartmentId,
            ProjectIds = employee.ProjectAssignments.Select(assignment => assignment.ProjectId).ToList(),
            AssignedProjects = employee.ProjectAssignments
                .Where(assignment => assignment.Project.IsActive)
                .OrderBy(assignment => assignment.Project.ProjectName)
                .Select(assignment => new ProjectAssignmentDto
                {
                    ProjectId = assignment.ProjectId,
                    ProjectName = assignment.Project.ProjectName
                })
                .ToList(),
            Department = employee.Department,
            Location = employee.Location,
            CreatedTime = employee.CreatedTime,
            DOB = employee.DOB,
            Salary = employee.Salary,
            Designation = employee.Designation,
            DesignationId = employee.DesignationId,
            Gender = employee.Gender,
            MaritalStatus = employee.MaritalStatus,
            BloodGroup = employee.BloodGroup,
            EmpIsActive = employee.EmpIsActive,
            SickLeaveAllowance = employee.SickLeaveAllowance,
            CasualLeaveAllowance = employee.CasualLeaveAllowance
        };

        private static Employee Map(EmployeeDto model) => new()
        {
            Id = model.Id,
            ReportingPersonId = model.ReportingPersonId,
            FirstName = model.FirstName,
            LastName = model.LastName,
            Email = model.Email,
            PersonalEmail = model.PersonalEmail,
            Skills = model.Skills,
            ProfilePhotoPath = model.ProfilePhotoPath,
            Phone = model.Phone,
            DepartmentId = model.DepartmentId,
            Department = model.Department,
            Location = model.Location,
            DOB = model.DOB?.Date,
            Salary = model.Salary,
            Designation = model.Designation,
            DesignationId = model.DesignationId,
            Gender = model.Gender,
            MaritalStatus = model.MaritalStatus,
            BloodGroup = model.BloodGroup,
            EmpIsActive = model.EmpIsActive,
            SickLeaveAllowance = model.SickLeaveAllowance,
            SickLeaveRemaining = model.SickLeaveAllowance,
            CasualLeaveAllowance = model.CasualLeaveAllowance,
            CasualLeaveRemaining = model.CasualLeaveAllowance,
        };
    }
}

