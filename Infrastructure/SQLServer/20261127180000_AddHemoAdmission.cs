using Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
namespace Infrastructure.SQLServer;
[DbContext(typeof(ApplicationDbContext))]
[Migration("20261127180000_AddHemoAdmission")]
public partial class AddHemoAdmission : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(name: "Patient", columns: table => new
        {
            GUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            Created = table.Column<DateTime>(type: "datetime2", nullable: false),
            ConvertID = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            CreatedByIP = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
            LastModified = table.Column<DateTime>(type: "datetime2", nullable: false),
            LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            LastModifiedByIP = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
            Deleted = table.Column<DateTime>(type: "datetime2", nullable: true),
            DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            DeletedByIP = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            IsDelete = table.Column<bool>(type: "bit", nullable: false),
            OrgLocationGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            IdentificationType = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
            IdentificationNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
            FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
            LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
            FatherName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
            BirthDate = table.Column<DateTime>(type: "datetime2", nullable: true),
            Gender = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
            Mobile = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_Patient", x=>x.GUID);
            table.ForeignKey("FK_Patient_OrgLocation_OrgLocationGUID", x=>x.OrgLocationGUID, "OrgLocation", "GUID", onDelete: ReferentialAction.Restrict);
        });
        migrationBuilder.CreateIndex(name: "IX_Patient_OrgLocationGUID", table: "Patient", column: "OrgLocationGUID");
        migrationBuilder.CreateIndex(name: "IX_Patient_OrgLocationGUID_IdentificationType_IdentificationNumber_IsDelete", table: "Patient", columns: new[] { "OrgLocationGUID", "IdentificationType", "IdentificationNumber", "IsDelete" }, unique: true);
        migrationBuilder.CreateTable(name: "PatientAdmission", columns: table => new
        {
            GUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            Created = table.Column<DateTime>(type: "datetime2", nullable: false),
            ConvertID = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            CreatedByIP = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
            LastModified = table.Column<DateTime>(type: "datetime2", nullable: false),
            LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            LastModifiedByIP = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
            Deleted = table.Column<DateTime>(type: "datetime2", nullable: true),
            DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            DeletedByIP = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            IsDelete = table.Column<bool>(type: "bit", nullable: false),
            PatientGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            OrgLocationGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            HISNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
            Ward = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
            BedNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
            Physician = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
            AdmissionType = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
            Diagnosis = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
            Insurance = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
            BloodGroup = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: true),
            TransfusionHistory = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
            Allergy = table.Column<bool>(type: "bit", nullable: false),
            Leukoreduced = table.Column<bool>(type: "bit", nullable: false),
            Irradiated = table.Column<bool>(type: "bit", nullable: false),
            Alloantibody = table.Column<bool>(type: "bit", nullable: false),
            Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
            AdmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
            IdentityStatus = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
            IdentityReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
            WristbandCode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
            Status = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
            HistoryReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
            ClearanceChecklist = table.Column<int>(type: "int", nullable: false),
            ClearanceNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
            ClearanceReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
            Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_PatientAdmission", x=>x.GUID);
            table.ForeignKey("FK_PatientAdmission_Patient_PatientGUID", x=>x.PatientGUID, "Patient", "GUID", onDelete: ReferentialAction.Restrict);
            table.ForeignKey("FK_PatientAdmission_OrgLocation_OrgLocationGUID", x=>x.OrgLocationGUID, "OrgLocation", "GUID", onDelete: ReferentialAction.Restrict);
        });
        migrationBuilder.CreateIndex(name: "IX_PatientAdmission_PatientGUID", table: "PatientAdmission", column: "PatientGUID");
        migrationBuilder.CreateIndex(name: "IX_PatientAdmission_OrgLocationGUID_HISNumber_IsDelete", table: "PatientAdmission", columns: new[] { "OrgLocationGUID", "HISNumber", "IsDelete" }, unique: true);
        migrationBuilder.CreateIndex(name: "IX_PatientAdmission_OrgLocationGUID_AdmittedAt", table: "PatientAdmission", columns: new[] { "OrgLocationGUID", "AdmittedAt" });
        migrationBuilder.CreateIndex(name: "IX_PatientAdmission_WristbandCode", table: "PatientAdmission", column: "WristbandCode", unique: true);
        migrationBuilder.CreateTable(name: "PatientAdmissionReview", columns: table => new
        {
            GUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            Created = table.Column<DateTime>(type: "datetime2", nullable: false),
            ConvertID = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            CreatedByIP = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
            LastModified = table.Column<DateTime>(type: "datetime2", nullable: false),
            LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            LastModifiedByIP = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
            Deleted = table.Column<DateTime>(type: "datetime2", nullable: true),
            DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            DeletedByIP = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            IsDelete = table.Column<bool>(type: "bit", nullable: false),
            PatientAdmissionGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            Kind = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
            Outcome = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
            Checklist = table.Column<int>(type: "int", nullable: false),
            Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
            ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_PatientAdmissionReview", x=>x.GUID);
            table.ForeignKey("FK_PatientAdmissionReview_PatientAdmission_PatientAdmissionGUID", x=>x.PatientAdmissionGUID, "PatientAdmission", "GUID", onDelete: ReferentialAction.Restrict);
        });
        migrationBuilder.CreateIndex(name: "IX_PatientAdmissionReview_PatientAdmissionGUID_ReviewedAt", table: "PatientAdmissionReview", columns: new[] { "PatientAdmissionGUID", "ReviewedAt" });
    }
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("PatientAdmissionReview");
        migrationBuilder.DropTable("PatientAdmission");
        migrationBuilder.DropTable("Patient");
    }
}
