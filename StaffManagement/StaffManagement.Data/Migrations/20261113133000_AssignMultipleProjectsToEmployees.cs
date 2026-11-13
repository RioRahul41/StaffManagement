using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StaffManagement.Data.Migrations
{
    public partial class AssignMultipleProjectsToEmployees : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmployeeProjects",
                columns: table => new
                {
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    ProjectId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeProjects", x => new { x.EmployeeId, x.ProjectId });
                    table.ForeignKey("FK_EmployeeProjects_Employees_EmployeeId", x => x.EmployeeId, "Employees", "Id", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_EmployeeProjects_Projects_ProjectId", x => x.ProjectId, "Projects", "ProjectId", onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeProjects_ProjectId",
                table: "EmployeeProjects",
                column: "ProjectId");

            migrationBuilder.Sql("IF COL_LENGTH('dbo.Employees', 'ProjectId') IS NOT NULL INSERT INTO EmployeeProjects (EmployeeId, ProjectId) SELECT e.Id, e.ProjectId FROM Employees AS e WHERE e.ProjectId IS NOT NULL;");

            migrationBuilder.DropForeignKey(name: "FK_Employees_Projects_ProjectId", table: "Employees");
            migrationBuilder.DropIndex(name: "IX_Employees_ProjectId", table: "Employees");
            migrationBuilder.DropColumn(name: "ProjectId", table: "Employees");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "Employees",
                type: "int",
                nullable: true);

            migrationBuilder.Sql("UPDATE e SET e.ProjectId = a.ProjectId FROM Employees AS e INNER JOIN (SELECT EmployeeId, MIN(ProjectId) AS ProjectId FROM EmployeeProjects GROUP BY EmployeeId) AS a ON a.EmployeeId = e.Id;");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_ProjectId",
                table: "Employees",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Projects_ProjectId",
                table: "Employees",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "ProjectId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.DropTable(name: "EmployeeProjects");
        }
    }
}
