using Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
namespace Infrastructure.SQLServer;
[DbContext(typeof(ApplicationDbContext))]
[Migration("20261128010000_AddHemoQualityManagement")]
public partial class AddHemoQualityManagement : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(name: "HemoEquipment", columns: table => new
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
            Code = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
            Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
            Category = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
            BrandModel = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
            Ward = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
            SerialNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
            Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
            CalibrationAt = table.Column<DateTime>(type: "datetime2", nullable: true),
            CalibrationDueAt = table.Column<DateTime>(type: "datetime2", nullable: true),
            MaintenanceDueAt = table.Column<DateTime>(type: "datetime2", nullable: true),
            Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_HemoEquipment", x=>x.GUID);
            table.ForeignKey("FK_HemoEquipment_OrgLocation_OrgLocationGUID", x=>x.OrgLocationGUID, "OrgLocation", "GUID", onDelete: ReferentialAction.Restrict);
        });
        migrationBuilder.CreateIndex(name: "IX_HemoEquipment_OrgLocationGUID_Code", table: "HemoEquipment", columns: new[] { "OrgLocationGUID", "Code" }, unique: true);
        migrationBuilder.CreateIndex(name: "IX_HemoEquipment_OrgLocationGUID_Status", table: "HemoEquipment", columns: new[] { "OrgLocationGUID", "Status" });
        migrationBuilder.CreateTable(name: "HemoEquipmentEvent", columns: table => new
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
            HemoEquipmentGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            ActorUserGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            ActorRoleGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            Kind = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
            PreviousStatus = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
            NewStatus = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
            OccurredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
            Notes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
            Reference = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
            Provider = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
            DueAt = table.Column<DateTime>(type: "datetime2", nullable: true),
            EvidenceJson = table.Column<string>(type: "nvarchar(max)", nullable: true)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_HemoEquipmentEvent", x=>x.GUID);
            table.ForeignKey("FK_HemoEquipmentEvent_HemoEquipment_HemoEquipmentGUID", x=>x.HemoEquipmentGUID, "HemoEquipment", "GUID", onDelete: ReferentialAction.Restrict);
        });
        migrationBuilder.CreateIndex(name: "IX_HemoEquipmentEvent_HemoEquipmentGUID_OccurredAt", table: "HemoEquipmentEvent", columns: new[] { "HemoEquipmentGUID", "OccurredAt" });
        migrationBuilder.CreateTable(name: "HemoEquipmentIncident", columns: table => new
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
            HemoEquipmentGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            IncidentNumber = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
            Urgency = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
            Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
            ReportedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
            ReporterUserGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
            Resolution = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
            ResolutionReference = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
            ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
            ResolvedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_HemoEquipmentIncident", x=>x.GUID);
            table.ForeignKey("FK_HemoEquipmentIncident_HemoEquipment_HemoEquipmentGUID", x=>x.HemoEquipmentGUID, "HemoEquipment", "GUID", onDelete: ReferentialAction.Restrict);
        });
        migrationBuilder.CreateIndex(name: "IX_HemoEquipmentIncident_IncidentNumber", table: "HemoEquipmentIncident", column: "IncidentNumber", unique: true);
        migrationBuilder.CreateIndex(name: "IX_HemoEquipmentIncident_HemoEquipmentGUID_Status", table: "HemoEquipmentIncident", columns: new[] { "HemoEquipmentGUID", "Status" });
        migrationBuilder.CreateTable(name: "HemoQualityRecord", columns: table => new
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
            Kind = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
            Number = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
            Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
            Ward = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
            Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
            OccurredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
            CreatedByUserGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            DetailsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
            ReviewNotes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
            ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
            ReviewedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_HemoQualityRecord", x=>x.GUID);
            table.ForeignKey("FK_HemoQualityRecord_OrgLocation_OrgLocationGUID", x=>x.OrgLocationGUID, "OrgLocation", "GUID", onDelete: ReferentialAction.Restrict);
        });
        migrationBuilder.CreateIndex(name: "IX_HemoQualityRecord_OrgLocationGUID_Number", table: "HemoQualityRecord", columns: new[] { "OrgLocationGUID", "Number" }, unique: true);
        migrationBuilder.CreateIndex(name: "IX_HemoQualityRecord_OrgLocationGUID_Kind_Status_OccurredAt", table: "HemoQualityRecord", columns: new[] { "OrgLocationGUID", "Kind", "Status", "OccurredAt" });
        migrationBuilder.CreateTable(name: "HemoQualityLine", columns: table => new
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
            HemoQualityRecordGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
            DetailsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
            Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
            OwnerUserGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            DueAt = table.Column<DateTime>(type: "datetime2", nullable: true),
            Progress = table.Column<int>(type: "int", nullable: false),
            Evidence = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
            VerifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            VerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
            Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_HemoQualityLine", x=>x.GUID);
            table.ForeignKey("FK_HemoQualityLine_HemoQualityRecord_HemoQualityRecordGUID", x=>x.HemoQualityRecordGUID, "HemoQualityRecord", "GUID", onDelete: ReferentialAction.Restrict);
        });
        migrationBuilder.CreateIndex(name: "IX_HemoQualityLine_HemoQualityRecordGUID_Status", table: "HemoQualityLine", columns: new[] { "HemoQualityRecordGUID", "Status" });
        migrationBuilder.CreateIndex(name: "IX_HemoQualityLine_OwnerUserGUID_Status", table: "HemoQualityLine", columns: new[] { "OwnerUserGUID", "Status" });
        migrationBuilder.CreateTable(name: "HemoQualityEvent", columns: table => new
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
            HemoQualityRecordGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            ActorUserGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            ActorRoleGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            Action = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
            PreviousStatus = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
            NewStatus = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
            OccurredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
            Notes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
            EvidenceJson = table.Column<string>(type: "nvarchar(max)", nullable: true)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_HemoQualityEvent", x=>x.GUID);
            table.ForeignKey("FK_HemoQualityEvent_HemoQualityRecord_HemoQualityRecordGUID", x=>x.HemoQualityRecordGUID, "HemoQualityRecord", "GUID", onDelete: ReferentialAction.Restrict);
        });
        migrationBuilder.CreateIndex(name: "IX_HemoQualityEvent_HemoQualityRecordGUID_OccurredAt", table: "HemoQualityEvent", columns: new[] { "HemoQualityRecordGUID", "OccurredAt" });
        migrationBuilder.CreateTable(name: "HemoFacilityProfile", columns: table => new
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
            FacilityName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
            AccreditationCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
            Chair = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
            Secretary = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
            Officer = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
            BloodCenter = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
            BloodCenterCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
            ReportFooter = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
            Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_HemoFacilityProfile", x=>x.GUID);
            table.ForeignKey("FK_HemoFacilityProfile_OrgLocation_OrgLocationGUID", x=>x.OrgLocationGUID, "OrgLocation", "GUID", onDelete: ReferentialAction.Restrict);
        });
        migrationBuilder.CreateIndex(name: "IX_HemoFacilityProfile_OrgLocationGUID", table: "HemoFacilityProfile", column: "OrgLocationGUID", unique: true);
        migrationBuilder.CreateTable(name: "HemoFacilityProfileEvent", columns: table => new
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
            HemoFacilityProfileGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            ActorUserGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            ActorRoleGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            OccurredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
            Notes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
            PreviousJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
            CurrentJson = table.Column<string>(type: "nvarchar(max)", nullable: false)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_HemoFacilityProfileEvent", x=>x.GUID);
            table.ForeignKey("FK_HemoFacilityProfileEvent_HemoFacilityProfile_HemoFacilityProfileGUID", x=>x.HemoFacilityProfileGUID, "HemoFacilityProfile", "GUID", onDelete: ReferentialAction.Restrict);
        });
        migrationBuilder.CreateIndex(name: "IX_HemoFacilityProfileEvent_HemoFacilityProfileGUID_OccurredAt", table: "HemoFacilityProfileEvent", columns: new[] { "HemoFacilityProfileGUID", "OccurredAt" });
    }
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("HemoFacilityProfileEvent");
        migrationBuilder.DropTable("HemoFacilityProfile");
        migrationBuilder.DropTable("HemoQualityEvent");
        migrationBuilder.DropTable("HemoQualityLine");
        migrationBuilder.DropTable("HemoQualityRecord");
        migrationBuilder.DropTable("HemoEquipmentIncident");
        migrationBuilder.DropTable("HemoEquipmentEvent");
        migrationBuilder.DropTable("HemoEquipment");
    }
}
