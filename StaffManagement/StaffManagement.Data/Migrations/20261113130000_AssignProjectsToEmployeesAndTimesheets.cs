using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StaffManagement.Data.Migrations
{
    public partial class AssignProjectsToEmployeesAndTimesheets : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "Employees",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "Timesheets",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_ProjectId",
                table: "Employees",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Timesheets_ProjectId",
                table: "Timesheets",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Projects_ProjectId",
                table: "Employees",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "ProjectId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Timesheets_Projects_ProjectId",
                table: "Timesheets",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "ProjectId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql("UPDATE t SET t.ProjectId = p.ProjectId FROM Timesheets AS t INNER JOIN Projects AS p ON LOWER(LTRIM(RTRIM(t.ProjectName))) = LOWER(LTRIM(RTRIM(p.ProjectName)));");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_Employees_Projects_ProjectId", table: "Employees");
            migrationBuilder.DropForeignKey(name: "FK_Timesheets_Projects_ProjectId", table: "Timesheets");
            migrationBuilder.DropIndex(name: "IX_Employees_ProjectId", table: "Employees");
            migrationBuilder.DropIndex(name: "IX_Timesheets_ProjectId", table: "Timesheets");
            migrationBuilder.DropColumn(name: "ProjectId", table: "Employees");
            migrationBuilder.DropColumn(name: "ProjectId", table: "Timesheets");
        }
    }
}
