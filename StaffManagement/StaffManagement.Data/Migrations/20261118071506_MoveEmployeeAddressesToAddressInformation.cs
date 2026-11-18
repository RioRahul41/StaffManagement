using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StaffManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class MoveEmployeeAddressesToAddressInformation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddUniqueConstraint(
                name: "AK_EmployeeProfiles_EmployeeId",
                table: "EmployeeProfiles",
                column: "EmployeeId");

            migrationBuilder.CreateTable(
                name: "EmployeeAddressInformations",
                columns: table => new
                {
                    EmployeeAddressInformationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    PermanentAddressLine1 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PermanentAddressLine2 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PermanentCountry = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PermanentState = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PermanentCity = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PermanentPincode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CurrentAddressLine1 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CurrentAddressLine2 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CurrentCountry = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CurrentState = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CurrentCity = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CurrentPincode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeAddressInformations", x => x.EmployeeAddressInformationId);
                    table.ForeignKey(
                        name: "FK_EmployeeAddressInformations_EmployeeProfiles_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "EmployeeProfiles",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAddressInformations_EmployeeId",
                table: "EmployeeAddressInformations",
                column: "EmployeeId",
                unique: true);

            migrationBuilder.Sql(@"
INSERT INTO EmployeeAddressInformations
    (EmployeeId, PermanentAddressLine1, PermanentAddressLine2, PermanentCountry, PermanentState, PermanentCity, PermanentPincode,
     CurrentAddressLine1, CurrentAddressLine2, CurrentCountry, CurrentState, CurrentCity, CurrentPincode)
SELECT EmployeeId, PermanentAddressLine1, PermanentAddressLine2, PermanentCountry, PermanentState, PermanentCity, PermanentPincode,
       CurrentAddressLine1, CurrentAddressLine2, CurrentCountry, CurrentState, CurrentCity, CurrentPincode
FROM EmployeeProfiles
WHERE PermanentAddressLine1 IS NOT NULL OR PermanentAddressLine2 IS NOT NULL OR PermanentCountry IS NOT NULL
   OR PermanentState IS NOT NULL OR PermanentCity IS NOT NULL OR PermanentPincode IS NOT NULL
   OR CurrentAddressLine1 IS NOT NULL OR CurrentAddressLine2 IS NOT NULL OR CurrentCountry IS NOT NULL
   OR CurrentState IS NOT NULL OR CurrentCity IS NOT NULL OR CurrentPincode IS NOT NULL;
");

            migrationBuilder.DropColumn(name: "CurrentAddressLine1", table: "EmployeeProfiles");
            migrationBuilder.DropColumn(name: "CurrentAddressLine2", table: "EmployeeProfiles");
            migrationBuilder.DropColumn(name: "CurrentCity", table: "EmployeeProfiles");
            migrationBuilder.DropColumn(name: "CurrentCountry", table: "EmployeeProfiles");
            migrationBuilder.DropColumn(name: "CurrentPincode", table: "EmployeeProfiles");
            migrationBuilder.DropColumn(name: "CurrentState", table: "EmployeeProfiles");
            migrationBuilder.DropColumn(name: "PermanentAddressLine1", table: "EmployeeProfiles");
            migrationBuilder.DropColumn(name: "PermanentAddressLine2", table: "EmployeeProfiles");
            migrationBuilder.DropColumn(name: "PermanentCity", table: "EmployeeProfiles");
            migrationBuilder.DropColumn(name: "PermanentCountry", table: "EmployeeProfiles");
            migrationBuilder.DropColumn(name: "PermanentPincode", table: "EmployeeProfiles");
            migrationBuilder.DropColumn(name: "PermanentState", table: "EmployeeProfiles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(name: "CurrentAddressLine1", table: "EmployeeProfiles", type: "nvarchar(200)", maxLength: 200, nullable: true);
            migrationBuilder.AddColumn<string>(name: "CurrentAddressLine2", table: "EmployeeProfiles", type: "nvarchar(200)", maxLength: 200, nullable: true);
            migrationBuilder.AddColumn<string>(name: "CurrentCity", table: "EmployeeProfiles", type: "nvarchar(100)", maxLength: 100, nullable: true);
            migrationBuilder.AddColumn<string>(name: "CurrentCountry", table: "EmployeeProfiles", type: "nvarchar(100)", maxLength: 100, nullable: true);
            migrationBuilder.AddColumn<string>(name: "CurrentPincode", table: "EmployeeProfiles", type: "nvarchar(20)", maxLength: 20, nullable: true);
            migrationBuilder.AddColumn<string>(name: "CurrentState", table: "EmployeeProfiles", type: "nvarchar(100)", maxLength: 100, nullable: true);
            migrationBuilder.AddColumn<string>(name: "PermanentAddressLine1", table: "EmployeeProfiles", type: "nvarchar(200)", maxLength: 200, nullable: true);
            migrationBuilder.AddColumn<string>(name: "PermanentAddressLine2", table: "EmployeeProfiles", type: "nvarchar(200)", maxLength: 200, nullable: true);
            migrationBuilder.AddColumn<string>(name: "PermanentCity", table: "EmployeeProfiles", type: "nvarchar(100)", maxLength: 100, nullable: true);
            migrationBuilder.AddColumn<string>(name: "PermanentCountry", table: "EmployeeProfiles", type: "nvarchar(100)", maxLength: 100, nullable: true);
            migrationBuilder.AddColumn<string>(name: "PermanentPincode", table: "EmployeeProfiles", type: "nvarchar(20)", maxLength: 20, nullable: true);
            migrationBuilder.AddColumn<string>(name: "PermanentState", table: "EmployeeProfiles", type: "nvarchar(100)", maxLength: 100, nullable: true);

            migrationBuilder.Sql(@"
UPDATE profile
SET CurrentAddressLine1 = address.CurrentAddressLine1,
    CurrentAddressLine2 = address.CurrentAddressLine2,
    CurrentCity = address.CurrentCity,
    CurrentCountry = address.CurrentCountry,
    CurrentPincode = address.CurrentPincode,
    CurrentState = address.CurrentState,
    PermanentAddressLine1 = address.PermanentAddressLine1,
    PermanentAddressLine2 = address.PermanentAddressLine2,
    PermanentCity = address.PermanentCity,
    PermanentCountry = address.PermanentCountry,
    PermanentPincode = address.PermanentPincode,
    PermanentState = address.PermanentState
FROM EmployeeProfiles AS profile
INNER JOIN EmployeeAddressInformations AS address ON address.EmployeeId = profile.EmployeeId;
");

            migrationBuilder.DropTable(name: "EmployeeAddressInformations");
            migrationBuilder.DropUniqueConstraint(name: "AK_EmployeeProfiles_EmployeeId", table: "EmployeeProfiles");
        }
    }
}
