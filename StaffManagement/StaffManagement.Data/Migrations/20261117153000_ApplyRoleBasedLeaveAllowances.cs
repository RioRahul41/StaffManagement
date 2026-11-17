using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StaffManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class ApplyRoleBasedLeaveAllowances : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ;WITH EmployeeRolePolicies AS
                (
                    SELECT
                        employee.Id AS EmployeeId,
                        CASE identityRole.Name
                            WHEN N'Admin' THEN 10
                            WHEN N'HR' THEN 8
                            WHEN N'Manager' THEN 7
                            WHEN N'User' THEN 6
                            WHEN N'Technical Support' THEN 7
                        END AS SickLeaveAllowance,
                        CASE identityRole.Name
                            WHEN N'Admin' THEN 20
                            WHEN N'HR' THEN 15
                            WHEN N'Manager' THEN 10
                            WHEN N'User' THEN 6
                            WHEN N'Technical Support' THEN 6
                        END AS CasualLeaveAllowance,
                        ROW_NUMBER() OVER
                        (
                            PARTITION BY employee.Id
                            ORDER BY CASE identityRole.Name
                                WHEN N'Admin' THEN 1
                                WHEN N'HR' THEN 2
                                WHEN N'Manager' THEN 3
                                WHEN N'User' THEN 4
                                WHEN N'Technical Support' THEN 5
                            END
                        ) AS RolePriority
                    FROM dbo.Employees AS employee
                    INNER JOIN dbo.AspNetUserRoles AS userRole
                        ON userRole.UserId = employee.IdentityUserId
                    INNER JOIN dbo.AspNetRoles AS identityRole
                        ON identityRole.Id = userRole.RoleId
                    WHERE identityRole.Name IN
                        (N'Admin', N'HR', N'Manager', N'User', N'Technical Support')
                )
                UPDATE employee
                SET
                    SickLeaveAllowance = policy.SickLeaveAllowance,
                    CasualLeaveAllowance = policy.CasualLeaveAllowance
                FROM dbo.Employees AS employee
                INNER JOIN EmployeeRolePolicies AS policy
                    ON policy.EmployeeId = employee.Id
                   AND policy.RolePriority = 1;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // This data correction is intentionally not reversed: restoring the old
            // defaults would overwrite employee leave balances.
        }
    }
}
