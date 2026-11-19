using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StaffManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DocumentTypeId",
                table: "EmployeeOtherDetails",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DocumentTypes",
                columns: table => new
                {
                    DocumentTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentTypeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RequiresValidityDates = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentTypes", x => x.DocumentTypeId);
                });

            migrationBuilder.Sql("""
                INSERT INTO [DocumentTypes] ([DocumentTypeName], [RequiresValidityDates], [IsActive], [CreatedAtUtc])
                VALUES
                    (N'Aadhaar', 0, 1, SYSUTCDATETIME()),
                    (N'Passport', 1, 1, SYSUTCDATETIME()),
                    (N'Other', 1, 1, SYSUTCDATETIME());

                INSERT INTO [DocumentTypes] ([DocumentTypeName], [RequiresValidityDates], [IsActive], [CreatedAtUtc])
                SELECT DISTINCT LTRIM(RTRIM(details.[DocumentType])),
                    CASE WHEN UPPER(LTRIM(RTRIM(details.[DocumentType]))) IN (N'AADHAAR', N'AADHAR') THEN 0 ELSE 1 END,
                    1, SYSUTCDATETIME()
                FROM [EmployeeOtherDetails] AS details
                WHERE NULLIF(LTRIM(RTRIM(details.[DocumentType])), N'') IS NOT NULL
                  AND NOT EXISTS (
                      SELECT 1 FROM [DocumentTypes] AS types
                      WHERE UPPER(types.[DocumentTypeName]) = UPPER(LTRIM(RTRIM(details.[DocumentType])))
                  );

                UPDATE details
                SET details.[DocumentTypeId] = types.[DocumentTypeId]
                FROM [EmployeeOtherDetails] AS details
                INNER JOIN [DocumentTypes] AS types
                    ON UPPER(types.[DocumentTypeName]) = UPPER(LTRIM(RTRIM(details.[DocumentType])));
                """);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeOtherDetails_DocumentTypeId",
                table: "EmployeeOtherDetails",
                column: "DocumentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTypes_DocumentTypeName",
                table: "DocumentTypes",
                column: "DocumentTypeName",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeOtherDetails_DocumentTypes_DocumentTypeId",
                table: "EmployeeOtherDetails",
                column: "DocumentTypeId",
                principalTable: "DocumentTypes",
                principalColumn: "DocumentTypeId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeOtherDetails_DocumentTypes_DocumentTypeId",
                table: "EmployeeOtherDetails");

            migrationBuilder.DropTable(
                name: "DocumentTypes");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeOtherDetails_DocumentTypeId",
                table: "EmployeeOtherDetails");

            migrationBuilder.DropColumn(
                name: "DocumentTypeId",
                table: "EmployeeOtherDetails");
        }
    }
}
