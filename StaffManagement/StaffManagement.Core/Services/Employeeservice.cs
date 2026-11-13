using StaffManagement.Core.DTOs.EmployeeModel;
using StaffManagement.Core.IRepositories;
using StaffManagement.Core.Iservices;
using StaffManagement.Model.Entities;

namespace StaffManagement.Core.Services
{
    public sealed class EmployeeService(
        IEmployeeRepository repository,
        IProjectRepository projectRepository) : IEmployeeService
    {
        public async Task<IReadOnlyList<EmployeeDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
            (await repository.GetAllAsync(cancellationToken)).Select(Map).ToList();

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

            employee.FirstName = model.FirstName;
            employee.LastName = model.LastName;
            employee.Email = model.Email;
            employee.Phone = model.Phone;
            employee.DepartmentId = model.DepartmentId;
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
            employee.DOB = model.DOB;
            employee.Salary = model.Salary;
            employee.Designation = model.Designation;
            employee.Gender = model.Gender;
            employee.EmpIsActive = model.EmpIsActive;
            employee.UpdatedTime = DateTime.UtcNow;

            repository.Update(employee);
            await repository.SaveChangesAsync(cancellationToken);
            return (true, null);
        }

        public async Task<(bool Succeeded, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var employee = await repository.GetByIdAsync(id, cancellationToken);
            if (employee is null)
                return (false, "Employee was not found.");

            repository.Remove(employee);
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
            DOB = employee.DOB,
            Salary = employee.Salary,
            Designation = employee.Designation,
            Gender = employee.Gender,
            EmpIsActive = employee.EmpIsActive
        };

        private static Employee Map(EmployeeDto model) => new()
        {
            Id = model.Id,
            ReportingPersonId = model.ReportingPersonId,
            FirstName = model.FirstName,
            LastName = model.LastName,
            Email = model.Email,
            Phone = model.Phone,
            DepartmentId = model.DepartmentId,
            Department = model.Department,
            Location = model.Location,
            DOB = model.DOB,
            Salary = model.Salary,
            Designation = model.Designation,
            Gender = model.Gender,
            EmpIsActive = model.EmpIsActive
        };
    }
}
