using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using StaffManagement.Data.DbContext;
using StaffManagement.Model.Entities;

#nullable disable

namespace StaffManagement.Data.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261114150000_AddLeaveManagement")]
public partial class AddLeaveManagement : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "ReportingPersonId",
            table: "Employees",
            type: "int",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_Employees_ReportingPersonId",
            table: "Employees",
            column: "ReportingPersonId");

        migrationBuilder.AddForeignKey(
            name: "FK_Employees_Employees_ReportingPersonId",
            table: "Employees",
            column: "ReportingPersonId",
            principalTable: "Employees",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.CreateTable(
            name: "LeaveRequests",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                EmployeeId = table.Column<int>(type: "int", nullable: false),
                LeaveType = table.Column<int>(type: "int", nullable: false),
                FromDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                ToDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                AppliedDays = table.Column<int>(type: "int", nullable: false),
                Reason = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                Status = table.Column<int>(type: "int", nullable: false),
                DocumentPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                CreatedTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedTime = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_LeaveRequests", x => x.Id);
                table.ForeignKey(
                    name: "FK_LeaveRequests_Employees_EmployeeId",
                    column: x => x.EmployeeId,
                    principalTable: "Employees",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_LeaveRequests_EmployeeId",
            table: "LeaveRequests",
            column: "EmployeeId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "LeaveRequests");

        migrationBuilder.DropForeignKey(
            name: "FK_Employees_Employees_ReportingPersonId",
            table: "Employees");

        migrationBuilder.DropIndex(
            name: "IX_Employees_ReportingPersonId",
            table: "Employees");

        migrationBuilder.DropColumn(
            name: "ReportingPersonId",
            table: "Employees");
    }

    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasAnnotation("ProductVersion", "10.0.15")
            .HasAnnotation("Relational:MaxIdentifierLength", 128);

        SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.ToTable("Employees");
            entity.HasOne(employee => employee.ReportingPerson)
                .WithMany()
                .HasForeignKey(employee => employee.ReportingPersonId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<LeaveRequest>(entity =>
        {
            entity.ToTable("LeaveRequests");
            entity.HasOne(request => request.Employee)
                .WithMany()
                .HasForeignKey(request => request.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
