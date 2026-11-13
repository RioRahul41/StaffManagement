using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StaffManagement.Data.Migrations
{
    public partial class RemoveTimesheetApprovalStatuses : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE Timesheets SET Status = CASE WHEN Status IN (0, 1, 4) THEN 1 WHEN Status IN (2, 3) THEN 2 ELSE 1 END;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE Timesheets SET Status = CASE WHEN Status = 1 THEN 1 WHEN Status = 2 THEN 2 ELSE 0 END;");
        }
    }
}
