using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StaffManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class LinkDesignationsToDepartmentsAndEmployees : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DesignationId",
                table: "Employees",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                table: "Designations",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(@"
UPDATE d
SET DepartmentId = m.DepartmentId
FROM Designations AS d
INNER JOIN (
    SELECT LTRIM(RTRIM(Designation)) AS DesignationName, MIN(DepartmentId) AS DepartmentId
    FROM Employees
    WHERE Designation IS NOT NULL AND DepartmentId IS NOT NULL
    GROUP BY LTRIM(RTRIM(Designation))
    HAVING COUNT(DISTINCT DepartmentId) = 1
) AS m ON LTRIM(RTRIM(d.DesignationName)) = m.DesignationName;

UPDATE e
SET DesignationId = d.DesignationId
FROM Employees AS e
INNER JOIN Designations AS d
    ON LTRIM(RTRIM(e.Designation)) = LTRIM(RTRIM(d.DesignationName))
   AND e.DepartmentId = d.DepartmentId;
" );
            migrationBuilder.CreateIndex(
                name: "IX_Employees_DesignationId",
                table: "Employees",
                column: "DesignationId");

            migrationBuilder.CreateIndex(
                name: "IX_Designations_DepartmentId",
                table: "Designations",
                column: "DepartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Designations_Departments_DepartmentId",
                table: "Designations",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "DepartmentId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Designations_DesignationId",
                table: "Employees",
                column: "DesignationId",
                principalTable: "Designations",
                principalColumn: "DesignationId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Designations_Departments_DepartmentId",
                table: "Designations");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Designations_DesignationId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_DesignationId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Designations_DepartmentId",
                table: "Designations");

            migrationBuilder.DropColumn(
                name: "DesignationId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "Designations");
        }
    }
}
