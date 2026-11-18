using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StaffManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class SyncEmployeeProfileFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
UPDATE profile
SET Gender = COALESCE(profile.Gender, employee.Gender),
    MaritalStatus = COALESCE(profile.MaritalStatus, employee.MaritalStatus),
    DOB = COALESCE(profile.DOB, employee.DOB),
    BloodGroup = COALESCE(profile.BloodGroup, employee.BloodGroup),
    PhoneNumber = COALESCE(profile.PhoneNumber, employee.Phone),
    EmergencyContactName = COALESCE(profile.EmergencyContactName, employee.EmergencyContactName),
    EmergencyContactRelationship = COALESCE(profile.EmergencyContactRelationship, employee.EmergencyContactRelationship),
    EmergencyContactPhone = COALESCE(profile.EmergencyContactPhone, employee.EmergencyContactPhone)
FROM EmployeeProfiles AS profile
INNER JOIN Employees AS employee ON employee.Id = profile.EmployeeId;

UPDATE employee
SET Gender = profile.Gender,
    MaritalStatus = profile.MaritalStatus,
    DOB = profile.DOB,
    BloodGroup = profile.BloodGroup,
    Phone = profile.PhoneNumber,
    EmergencyContactName = profile.EmergencyContactName,
    EmergencyContactRelationship = profile.EmergencyContactRelationship,
    EmergencyContactPhone = profile.EmergencyContactPhone
FROM Employees AS employee
INNER JOIN EmployeeProfiles AS profile ON profile.EmployeeId = employee.Id;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Data synchronization is intentionally irreversible; the original mismatched values cannot be reconstructed.
        }
    }
}
