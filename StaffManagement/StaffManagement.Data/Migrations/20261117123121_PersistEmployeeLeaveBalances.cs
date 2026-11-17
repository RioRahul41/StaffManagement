using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StaffManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class PersistEmployeeLeaveBalances : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CasualLeaveRemaining",
                table: "Employees",
                type: "int",
                nullable: false,
                defaultValue: 10);

            migrationBuilder.AddColumn<int>(
                name: "SickLeaveRemaining",
                table: "Employees",
                type: "int",
                nullable: false,
                defaultValue: 10);
            migrationBuilder.Sql("""
                ;WITH LeaveTotals AS
                (
                    SELECT
                        EmployeeId,
                        SUM(CASE WHEN (LOWER(LTRIM(RTRIM(LeaveTypeName))) = N'sick' OR (LeaveTypeName IS NULL AND LeaveType = 1)) AND Status = 1 THEN AppliedDays ELSE 0 END) AS SickUsed,
                        SUM(CASE WHEN (LOWER(LTRIM(RTRIM(LeaveTypeName))) = N'sick' OR (LeaveTypeName IS NULL AND LeaveType = 1)) AND Status = 0 THEN AppliedDays ELSE 0 END) AS SickPending,
                        SUM(CASE WHEN (LOWER(LTRIM(RTRIM(LeaveTypeName))) = N'casual' OR (LeaveTypeName IS NULL AND LeaveType = 2)) AND Status = 1 THEN AppliedDays ELSE 0 END) AS CasualUsed,
                        SUM(CASE WHEN (LOWER(LTRIM(RTRIM(LeaveTypeName))) = N'casual' OR (LeaveTypeName IS NULL AND LeaveType = 2)) AND Status = 0 THEN AppliedDays ELSE 0 END) AS CasualPending
                    FROM LeaveRequests
                    GROUP BY EmployeeId
                )
                UPDATE employee
                SET SickLeaveRemaining = CASE
                        WHEN employee.SickLeaveAllowance - COALESCE(totals.SickUsed, 0) - COALESCE(totals.SickPending, 0) < 0 THEN 0
                        ELSE employee.SickLeaveAllowance - COALESCE(totals.SickUsed, 0) - COALESCE(totals.SickPending, 0)
                    END,
                    CasualLeaveRemaining = CASE
                        WHEN employee.CasualLeaveAllowance - COALESCE(totals.CasualUsed, 0) - COALESCE(totals.CasualPending, 0) < 0 THEN 0
                        ELSE employee.CasualLeaveAllowance - COALESCE(totals.CasualUsed, 0) - COALESCE(totals.CasualPending, 0)
                    END
                FROM Employees AS employee
                LEFT JOIN LeaveTotals AS totals ON totals.EmployeeId = employee.Id;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CasualLeaveRemaining",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "SickLeaveRemaining",
                table: "Employees");
        }
    }
}
