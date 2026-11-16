using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using StaffManagement.Data.DbContext;

#nullable disable

namespace StaffManagement.Data.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261116090000_AddPolicyDocuments")]
public sealed class AddPolicyDocuments : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "PolicyDocuments",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                PolicyName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                OriginalFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                StoredFileName = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                UploadedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                UploadedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_PolicyDocuments", x => x.Id));
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "PolicyDocuments");
    }
}
