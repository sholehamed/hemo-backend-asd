using Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
namespace Infrastructure.SQLServer;
[DbContext(typeof(ApplicationDbContext))]
[Migration("20261127200000_AddHemoBloodRequests")]
public partial class AddHemoBloodRequests : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(name: "BloodRequest", columns: table => new
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
            ClientRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            RequestNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
            RequesterUserGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            RequesterRoleGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            Source = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
            FormType = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
            Priority = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
            Status = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
            SubmissionAttempt = table.Column<int>(type: "int", nullable: false),
            SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
            AdmissionVersionAtSubmission = table.Column<byte[]>(type: "varbinary(8)", maxLength: 8, nullable: true),
            Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_BloodRequest", x=>x.GUID);
            table.ForeignKey("FK_BloodRequest_PatientAdmission_PatientAdmissionGUID", x=>x.PatientAdmissionGUID, "PatientAdmission", "GUID", onDelete: ReferentialAction.Restrict);
        });
        migrationBuilder.CreateIndex(name: "IX_BloodRequest_PatientAdmissionGUID_Status", table: "BloodRequest", columns: new[] { "PatientAdmissionGUID", "Status" });
        migrationBuilder.CreateIndex(name: "IX_BloodRequest_ClientRequestId", table: "BloodRequest", column: "ClientRequestId", unique: true);
        migrationBuilder.CreateIndex(name: "IX_BloodRequest_RequestNumber", table: "BloodRequest", column: "RequestNumber", unique: true);
        migrationBuilder.CreateIndex(name: "IX_BloodRequest_RequesterUserGUID", table: "BloodRequest", column: "RequesterUserGUID");
        migrationBuilder.CreateIndex(name: "IX_BloodRequest_Status_SubmittedAt", table: "BloodRequest", columns: new[] { "Status", "SubmittedAt" });
        migrationBuilder.CreateTable(name: "BloodRequestItem", columns: table => new
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
            BloodRequestGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            ProductCode = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
            Quantity = table.Column<int>(type: "int", nullable: false),
            RequestedBloodGroup = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: true),
            SpecialInstructions = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_BloodRequestItem", x=>x.GUID);
            table.ForeignKey("FK_BloodRequestItem_BloodRequest_BloodRequestGUID", x=>x.BloodRequestGUID, "BloodRequest", "GUID", onDelete: ReferentialAction.Restrict);
        });
        migrationBuilder.CreateIndex(name: "IX_BloodRequestItem_BloodRequestGUID_ProductCode_IsDelete", table: "BloodRequestItem", columns: new[] { "BloodRequestGUID", "ProductCode", "IsDelete" }, unique: true);
        migrationBuilder.CreateTable(name: "RequestClinicalData", columns: table => new
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
            BloodRequestGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            Diagnosis = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
            ClinicalIndication = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
            Hemoglobin = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: true),
            Hematocrit = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: true),
            PlateletCount = table.Column<int>(type: "int", nullable: true),
            RecentTransfusion = table.Column<bool>(type: "bit", nullable: true),
            RecentPregnancy = table.Column<bool>(type: "bit", nullable: true),
            PriorReaction = table.Column<bool>(type: "bit", nullable: true),
            UnexpectedAntibody = table.Column<bool>(type: "bit", nullable: true),
            Premedication = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
            SampleStatus = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
            Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_RequestClinicalData", x=>x.GUID);
            table.ForeignKey("FK_RequestClinicalData_BloodRequest_BloodRequestGUID", x=>x.BloodRequestGUID, "BloodRequest", "GUID", onDelete: ReferentialAction.Restrict);
        });
        migrationBuilder.CreateIndex(name: "IX_RequestClinicalData_BloodRequestGUID", table: "RequestClinicalData", column: "BloodRequestGUID", unique: true);
        migrationBuilder.CreateTable(name: "RequestApproval", columns: table => new
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
            BloodRequestGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            ActorUserGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            ActorRoleGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            Action = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
            PreviousStatus = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
            NewStatus = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
            Reason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
            SubmissionAttempt = table.Column<int>(type: "int", nullable: false),
            OccurredAt = table.Column<DateTime>(type: "datetime2", nullable: false)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_RequestApproval", x=>x.GUID);
            table.ForeignKey("FK_RequestApproval_BloodRequest_BloodRequestGUID", x=>x.BloodRequestGUID, "BloodRequest", "GUID", onDelete: ReferentialAction.Restrict);
        });
        migrationBuilder.CreateIndex(name: "IX_RequestApproval_BloodRequestGUID_OccurredAt", table: "RequestApproval", columns: new[] { "BloodRequestGUID", "OccurredAt" });
    }
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("RequestApproval");
        migrationBuilder.DropTable("RequestClinicalData");
        migrationBuilder.DropTable("BloodRequestItem");
        migrationBuilder.DropTable("BloodRequest");
    }
}
