using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StaffManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLeaveTypeMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LeaveTypeName",
                table: "LeaveRequests",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LeaveTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveTypes", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "LeaveTypes",
                columns: new[] { "Id", "Name", "IsActive", "CreatedDate", "UpdatedDate" },
                values: new object[,]
                {
                    { 1, "Annual", true, new DateTime(2026, 11, 16, 0, 0, 0, DateTimeKind.Utc), null },
                    { 2, "Sick", true, new DateTime(2026, 11, 16, 0, 0, 0, DateTimeKind.Utc), null },
                    { 3, "Casual", true, new DateTime(2026, 11, 16, 0, 0, 0, DateTimeKind.Utc), null },
                    { 4, "Unpaid", true, new DateTime(2026, 11, 16, 0, 0, 0, DateTimeKind.Utc), null },
                    { 5, "Other", true, new DateTime(2026, 11, 16, 0, 0, 0, DateTimeKind.Utc), null }
                });

            migrationBuilder.Sql("UPDATE [LeaveRequests] SET [LeaveTypeName] = CASE [LeaveType] WHEN 0 THEN N'Annual' WHEN 1 THEN N'Sick' WHEN 2 THEN N'Casual' WHEN 3 THEN N'Unpaid' ELSE N'Other' END WHERE [LeaveTypeName] IS NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LeaveTypes");

            migrationBuilder.DropColumn(
                name: "LeaveTypeName",
                table: "LeaveRequests");
        }
    }
}
