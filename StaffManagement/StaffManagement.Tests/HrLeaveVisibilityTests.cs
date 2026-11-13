using StaffManagement.Core.DTOs.LeaveModel;
using StaffManagement.Core.IRepositories;
using StaffManagement.Core.Services;
using StaffManagement.Model.Entities;
using StaffManagement.Model.Enums;
using Xunit;

namespace StaffManagement.Tests;

public sealed class HrLeaveVisibilityTests
{
    [Fact]
    public async Task Hr_can_see_leave_requests_submitted_by_other_employees()
    {
        var hrEmployee = new Employee
        {
            Id = 1,
            FirstName = "Helen",
            LastName = "HR",
            Email = "hr@staffmanagement.local",
            IdentityUserId = "hr-user",
            Location = "HQ",
            EmpIsActive = true
        };
        var staffEmployee = new Employee
        {
            Id = 2,
            FirstName = "Sam",
            LastName = "Staff",
            Email = "sam@staffmanagement.local",
            IdentityUserId = "staff-user",
            Location = "HQ",
            EmpIsActive = true
        };
        var employeeRepository = new StubEmployeeRepository(hrEmployee);
        var leaveRepository = new StubLeaveRequestRepository(hrEmployee, staffEmployee);
        var service = new LeaveRequestService(leaveRepository, employeeRepository);

        var result = await service.GetPagedAsync(
            identityUserId: "hr-user",
            canReviewAssignedRequests: false,
            canViewAllRequests: true,
            pageNumber: 1,
            pageSize: 10);

        Assert.Contains(result.Items, request => request.EmployeeId == staffEmployee.Id);
        Assert.Contains(result.Items, request => request.EmployeeName == "Sam Staff");
    }

    private sealed class StubEmployeeRepository(Employee currentEmployee) : IEmployeeRepository
    {
        public Task<Employee?> GetByIdentityUserIdAsync(string identityUserId, CancellationToken cancellationToken = default) =>
            Task.FromResult<Employee?>(identityUserId == currentEmployee.IdentityUserId ? currentEmployee : null);

        public Task<IReadOnlyList<Employee>> GetAllAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<Employee?> GetByIdAsync(int id, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task AddAsync(Employee employee, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public void Update(Employee employee) => throw new NotSupportedException();
        public void Remove(Employee employee) => throw new NotSupportedException();
        public Task<bool> EmailExistsAsync(string email, int? excludedId = null, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }

    private sealed class StubLeaveRequestRepository(Employee hrEmployee, Employee staffEmployee) : ILeaveRequestRepository
    {
        private readonly LeaveRequest[] requests =
        [
            new LeaveRequest { Id = 1, EmployeeId = hrEmployee.Id, Employee = hrEmployee, LeaveType = LeaveType.Annual, FromDate = DateTime.Today, ToDate = DateTime.Today, AppliedDays = 1, Reason = "Personal", Status = LeaveStatus.Pending },
            new LeaveRequest { Id = 2, EmployeeId = staffEmployee.Id, Employee = staffEmployee, LeaveType = LeaveType.Annual, FromDate = DateTime.Today, ToDate = DateTime.Today, AppliedDays = 1, Reason = "Family appointment", Status = LeaveStatus.Pending }
        ];

        public Task<(IReadOnlyList<LeaveRequest> Items, int TotalCount)> GetPagedAsync(
            int? employeeId,
            bool includeAssignedRequests,
            bool includeAllRequests,
            int pageNumber,
            int pageSize,
            string? search = null,
            LeaveStatus? status = null,
            LeaveType? leaveType = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            CancellationToken cancellationToken = default)
        {
            var visibleRequests = requests.Where(request =>
                includeAllRequests ||
                (employeeId.HasValue && request.EmployeeId == employeeId.Value) ||
                (includeAssignedRequests && employeeId.HasValue && request.Employee.ReportingPersonId == employeeId.Value)).ToArray();
            return Task.FromResult(((IReadOnlyList<LeaveRequest>)visibleRequests, visibleRequests.Length));
        }

        public Task<LeaveRequest?> GetByIdAsync(int id, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task AddAsync(LeaveRequest request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public void Update(LeaveRequest request) => throw new NotSupportedException();
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
