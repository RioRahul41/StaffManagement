using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StaffManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateEmployeeProfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmployeeProfiles",
                columns: table => new
                {
                    EmployeeProfileId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    Gender = table.Column<int>(type: "int", nullable: true),
                    MaritalStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    DOB = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BloodGroup = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmergencyContactName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EmergencyContactRelationship = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EmergencyContactPhone = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeProfiles", x => x.EmployeeProfileId);
                    table.ForeignKey(
                        name: "FK_EmployeeProfiles_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql(@"
                INSERT INTO [EmployeeProfiles]
                    ([EmployeeId], [Gender], [MaritalStatus], [DOB], [BloodGroup], [PhoneNumber], [EmergencyContactName], [EmergencyContactRelationship], [EmergencyContactPhone])
                SELECT [Id], [Gender], [MaritalStatus], [DOB], [BloodGroup], [Phone], [EmergencyContactName], [EmergencyContactRelationship], [EmergencyContactPhone]
                FROM [Employees]
                WHERE [Gender] IS NOT NULL OR [MaritalStatus] IS NOT NULL OR [DOB] IS NOT NULL OR [BloodGroup] IS NOT NULL
                   OR [Phone] IS NOT NULL OR [EmergencyContactName] IS NOT NULL OR [EmergencyContactRelationship] IS NOT NULL OR [EmergencyContactPhone] IS NOT NULL;
                ");
            migrationBuilder.CreateIndex(
                name: "IX_EmployeeProfiles_EmployeeId",
                table: "EmployeeProfiles",
                column: "EmployeeId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeProfiles");
        }
    }
}

