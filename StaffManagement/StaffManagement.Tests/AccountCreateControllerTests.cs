using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using StaffManagement.Controllers;
using StaffManagement.Core.DTOs.EmployeeModel;
using StaffManagement.Core.IServices;
using StaffManagement.Core.Iservices;
using StaffManagement.Core.Security;
using StaffManagement.Model.Entities;
using Xunit;

namespace StaffManagement.Tests;

public sealed class AccountCreateControllerTests
{
    [Fact]
    public async Task Create_page_populates_reporting_person_options()
    {
        var controller = new AccountController(
            signInManager: null!,
            userManager: new TestUserManager(),
            accountService: new StubAccountService(),
            designationService: new StubDesignationService(),
            departmentService: new StubDepartmentService(),
            employeeService: new StubEmployeeService());

        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.Role, RoleNames.Admin)],
            authenticationType: "Test");
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity)
            }
        };

        var result = await controller.Create();

        Assert.IsType<ViewResult>(result);
        var reportingPersons = Assert.IsAssignableFrom<IReadOnlyList<EmployeeDto>>(controller.ViewBag.Managers);
        Assert.NotEmpty(reportingPersons);
    }

    private sealed class StubAccountService : IAccountService
    {
        public IReadOnlyList<string> GetCreatableRoles(IReadOnlyCollection<string> creatorRoles) => [RoleNames.User];

        public Task<AccountCreationResult> CreateAsync(
            CreateAccountDto model,
            IReadOnlyCollection<string> creatorRoles,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new AccountCreationResult(false, "Not used by this test."));
    }

    private sealed class StubDesignationService : IDesignationService
    {
        public Task<IReadOnlyList<Designation>> GetAllAsync() => Task.FromResult<IReadOnlyList<Designation>>([]);
        public Task<Designation?> GetByIdAsync(int designationId) => throw new NotSupportedException();
        public Task<(bool Succeeded, string? Error)> CreateAsync(Designation designation) => throw new NotSupportedException();
        public Task<(bool Succeeded, string? Error)> UpdateAsync(Designation designation) => throw new NotSupportedException();
        public Task<bool> DeleteAsync(int designationId) => throw new NotSupportedException();
    }

    private sealed class StubDepartmentService : IDepartmentService
    {
        public Task<IReadOnlyList<Department>> GetAllAsync() => Task.FromResult<IReadOnlyList<Department>>([]);
        public Task<IReadOnlyList<Department>> GetAllForSelectionAsync() => Task.FromResult<IReadOnlyList<Department>>([]);
        public Task<Department?> GetByIdAsync(int departmentId) => throw new NotSupportedException();
        public Task<(bool Succeeded, string? Error)> CreateAsync(Department department) => throw new NotSupportedException();
        public Task<(bool Succeeded, string? Error)> UpdateAsync(Department department) => throw new NotSupportedException();
        public Task<bool> DeleteAsync(int departmentId) => throw new NotSupportedException();
    }

    private sealed class StubEmployeeService : IEmployeeService
    {
        public Task<(IReadOnlyList<EmployeeDto> Items, int TotalCount)> GetPageAsync(
            string? searchTerm,
            int? departmentId,
            string? designation,
            bool? isActive,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task<IReadOnlyList<EmployeeDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<EmployeeDto>>(
            [
                new EmployeeDto
                {
                    Id = 12,
                    IdentityUserId = "manager-user-id",
                    FirstName = "Maya",
                    LastName = "Manager",
                    Email = "maya@staffmanagement.local",
                    EmpIsActive = true
                }
            ]);

        public Task<EmployeeDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<EmployeeDto?> GetByIdentityUserIdAsync(string identityUserId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<(bool Succeeded, string? Error)> CreateAsync(EmployeeDto model, string identityUserId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<(bool Succeeded, string? Error)> UpdateAsync(EmployeeDto model, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<(bool Succeeded, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }

    private sealed class TestUserManager : UserManager<ApplicationIdentity>
    {
        public TestUserManager()
            : base(
                new TestUserStore(),
                Options.Create(new IdentityOptions()),
                new PasswordHasher<ApplicationIdentity>(),
                [],
                [],
                new UpperInvariantLookupNormalizer(),
                new IdentityErrorDescriber(),
                null!,
                NullLogger<UserManager<ApplicationIdentity>>.Instance)
        {
        }

        public override Task<IList<ApplicationIdentity>> GetUsersInRoleAsync(string roleName) =>
            Task.FromResult<IList<ApplicationIdentity>>(
            [
                new ApplicationIdentity
                {
                    Id = "manager-user-id",
                    Email = "maya@staffmanagement.local"
                }
            ]);
    }

    private sealed class TestUserStore : IUserStore<ApplicationIdentity>
    {
        public Task<IdentityResult> CreateAsync(ApplicationIdentity user, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<IdentityResult> UpdateAsync(ApplicationIdentity user, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<IdentityResult> DeleteAsync(ApplicationIdentity user, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<ApplicationIdentity?> FindByIdAsync(string userId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<ApplicationIdentity?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken) => throw new NotSupportedException();
        public string? GetUserId(ApplicationIdentity user) => user.Id;
        public string? GetUserName(ApplicationIdentity user) => user.UserName;
        public Task SetUserNameAsync(ApplicationIdentity user, string? userName, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<string?> GetUserNameAsync(ApplicationIdentity user, CancellationToken cancellationToken) => Task.FromResult(user.UserName);
        public Task SetNormalizedUserNameAsync(ApplicationIdentity user, string? normalizedName, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<string?> GetNormalizedUserNameAsync(ApplicationIdentity user, CancellationToken cancellationToken) => Task.FromResult(user.NormalizedUserName);
        public void Dispose()
        {
        }
    }
}
