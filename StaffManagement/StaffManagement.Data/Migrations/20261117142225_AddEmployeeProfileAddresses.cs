using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StaffManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeProfileAddresses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CurrentAddressLine1",
                table: "EmployeeProfiles",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CurrentAddressLine2",
                table: "EmployeeProfiles",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CurrentCity",
                table: "EmployeeProfiles",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CurrentCountry",
                table: "EmployeeProfiles",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CurrentPincode",
                table: "EmployeeProfiles",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CurrentState",
                table: "EmployeeProfiles",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PermanentAddressLine1",
                table: "EmployeeProfiles",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PermanentAddressLine2",
                table: "EmployeeProfiles",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PermanentCity",
                table: "EmployeeProfiles",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PermanentCountry",
                table: "EmployeeProfiles",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PermanentPincode",
                table: "EmployeeProfiles",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PermanentState",
                table: "EmployeeProfiles",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentAddressLine1",
                table: "EmployeeProfiles");

            migrationBuilder.DropColumn(
                name: "CurrentAddressLine2",
                table: "EmployeeProfiles");

            migrationBuilder.DropColumn(
                name: "CurrentCity",
                table: "EmployeeProfiles");

            migrationBuilder.DropColumn(
                name: "CurrentCountry",
                table: "EmployeeProfiles");

            migrationBuilder.DropColumn(
                name: "CurrentPincode",
                table: "EmployeeProfiles");

            migrationBuilder.DropColumn(
                name: "CurrentState",
                table: "EmployeeProfiles");

            migrationBuilder.DropColumn(
                name: "PermanentAddressLine1",
                table: "EmployeeProfiles");

            migrationBuilder.DropColumn(
                name: "PermanentAddressLine2",
                table: "EmployeeProfiles");

            migrationBuilder.DropColumn(
                name: "PermanentCity",
                table: "EmployeeProfiles");

            migrationBuilder.DropColumn(
                name: "PermanentCountry",
                table: "EmployeeProfiles");

            migrationBuilder.DropColumn(
                name: "PermanentPincode",
                table: "EmployeeProfiles");

            migrationBuilder.DropColumn(
                name: "PermanentState",
                table: "EmployeeProfiles");
        }
    }
}
