using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StaffManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTimesheetFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Timesheets");

            migrationBuilder.DropColumn(
                name: "HoursWorked",
                table: "Timesheets");

            migrationBuilder.RenameColumn(
                name: "WorkDate",
                table: "Timesheets",
                newName: "StartDate");

            migrationBuilder.AddColumn<string>(
                name: "DocumentPath",
                table: "Timesheets",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "EndTime",
                table: "Timesheets",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<string>(
                name: "ProjectName",
                table: "Timesheets",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "StartTime",
                table: "Timesheets",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Timesheets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TaskName",
                table: "Timesheets",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DocumentPath",
                table: "Timesheets");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "Timesheets");

            migrationBuilder.DropColumn(
                name: "ProjectName",
                table: "Timesheets");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "Timesheets");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Timesheets");

            migrationBuilder.DropColumn(
                name: "TaskName",
                table: "Timesheets");

            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "Timesheets",
                newName: "WorkDate");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Timesheets",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "HoursWorked",
                table: "Timesheets",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }
    }
}
