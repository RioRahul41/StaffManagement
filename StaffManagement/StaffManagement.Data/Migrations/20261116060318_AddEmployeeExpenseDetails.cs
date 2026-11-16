using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StaffManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeExpenseDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "BillAmount",
                table: "EmployeeExpenses",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.Sql("UPDATE [EmployeeExpenses] SET [BillAmount] = [Amount]");

            migrationBuilder.AddColumn<string>(
                name: "BillNumber",
                table: "EmployeeExpenses",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "Legacy");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "EmployeeExpenses",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "Unspecified");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "EmployeeExpenses",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocumentPath",
                table: "EmployeeExpenses",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BillAmount",
                table: "EmployeeExpenses");

            migrationBuilder.DropColumn(
                name: "BillNumber",
                table: "EmployeeExpenses");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "EmployeeExpenses");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "EmployeeExpenses");

            migrationBuilder.DropColumn(
                name: "DocumentPath",
                table: "EmployeeExpenses");
        }
    }
}
