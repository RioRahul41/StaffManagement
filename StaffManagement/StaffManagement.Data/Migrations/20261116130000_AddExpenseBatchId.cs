using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using StaffManagement.Data.DbContext;

#nullable disable

namespace StaffManagement.Data.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261116130000_AddExpenseBatchId")]
public sealed class AddExpenseBatchId : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "BatchId",
            table: "EmployeeExpenses",
            type: "uniqueidentifier",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_EmployeeExpenses_EmployeeId_BatchId",
            table: "EmployeeExpenses",
            columns: new[] { "EmployeeId", "BatchId" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_EmployeeExpenses_EmployeeId_BatchId",
            table: "EmployeeExpenses");

        migrationBuilder.DropColumn(
            name: "BatchId",
            table: "EmployeeExpenses");
    }
}
