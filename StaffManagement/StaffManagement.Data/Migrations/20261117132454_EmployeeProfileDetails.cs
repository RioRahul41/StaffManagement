using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StaffManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class EmployeeProfileDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PersonalEmail",
                table: "Employees",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfilePhotoPath",
                table: "Employees",
                type: "nvarchar(260)",
                maxLength: 260,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Skills",
                table: "Employees",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PersonalEmail",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "ProfilePhotoPath",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "Skills",
                table: "Employees");
        }
    }
}
