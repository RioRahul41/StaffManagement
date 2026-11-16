using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using StaffManagement.Data.DbContext;

#nullable disable

namespace StaffManagement.Data.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261116100000_AddHolidays")]
public sealed class AddHolidays : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Holidays",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                HolidayName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                HolidayDate = table.Column<DateTime>(type: "date", nullable: false),
                Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Holidays", x => x.Id));

        migrationBuilder.CreateIndex(
            name: "IX_Holidays_HolidayDate",
            table: "Holidays",
            column: "HolidayDate");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Holidays");
    }
}
