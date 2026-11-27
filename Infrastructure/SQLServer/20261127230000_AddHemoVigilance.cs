using Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
namespace Infrastructure.SQLServer;
[DbContext(typeof(ApplicationDbContext))]
[Migration("20261127230000_AddHemoVigilance")]
public partial class AddHemoVigilance : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(name: "HemoReaction", columns: table => new
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
            BloodTransferGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            ClientReportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            CaseNumber = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
            Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
            OnsetAt = table.Column<DateTime>(type: "datetime2", nullable: false),
            ReportedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
            ReporterUserGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            SymptomsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
            InitialSeverity = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
            InitialNotes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
            ActionsTaken = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
            SamplesSent = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
            VolumeMl = table.Column<decimal>(type: "decimal(9,2)", precision: 9, scale: 2, nullable: true),
            DoctorUserGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            DoctorReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
            DiagnosisCode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
            Severity = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
            Imputability = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
            Outcome = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
            DoctorNotes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
            DoctorActions = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
            DoctorLabRequest = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
            FutureRecommendations = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
            DoctorReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
            BankUserGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            BankReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
            BankFindings = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
            BankSampleReference = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
            BankReportReference = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
            BankDisposition = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
            InvestigationNotes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
            ExternalStatus = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
            ExternalReference = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
            RcaRequired = table.Column<bool>(type: "bit", nullable: true),
            RcaLead = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
            RcaMethod = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
            RcaMeetingAt = table.Column<DateTime>(type: "datetime2", nullable: true),
            RcaParticipants = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
            DirectCause = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
            RootCause = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
            RcaPublishedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
            RcaPublishedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            ClosedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
            ClosedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_HemoReaction", x=>x.GUID);
            table.ForeignKey("FK_HemoReaction_OrgLocation_OrgLocationGUID", x=>x.OrgLocationGUID, "OrgLocation", "GUID", onDelete: ReferentialAction.Restrict);
            table.ForeignKey("FK_HemoReaction_BloodTransfer_BloodTransferGUID", x=>x.BloodTransferGUID, "BloodTransfer", "GUID", onDelete: ReferentialAction.Restrict);
        });
        migrationBuilder.CreateIndex(name: "IX_HemoReaction_ClientReportId", table: "HemoReaction", column: "ClientReportId", unique: true);
        migrationBuilder.CreateIndex(name: "IX_HemoReaction_OrgLocationGUID_CaseNumber", table: "HemoReaction", columns: new[] { "OrgLocationGUID", "CaseNumber" }, unique: true);
        migrationBuilder.CreateIndex(name: "IX_HemoReaction_OrgLocationGUID_Status_OnsetAt", table: "HemoReaction", columns: new[] { "OrgLocationGUID", "Status", "OnsetAt" });
        migrationBuilder.CreateIndex(name: "IX_HemoReaction_BloodTransferGUID_Status", table: "HemoReaction", columns: new[] { "BloodTransferGUID", "Status" });
        migrationBuilder.CreateTable(name: "HemoReactionEvent", columns: table => new
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
            HemoReactionGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            ActorUserGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            ActorRoleGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            Action = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
            PreviousStatus = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
            NewStatus = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
            OccurredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
            Notes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
            EvidenceJson = table.Column<string>(type: "nvarchar(max)", nullable: true)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_HemoReactionEvent", x=>x.GUID);
            table.ForeignKey("FK_HemoReactionEvent_HemoReaction_HemoReactionGUID", x=>x.HemoReactionGUID, "HemoReaction", "GUID", onDelete: ReferentialAction.Restrict);
        });
        migrationBuilder.CreateIndex(name: "IX_HemoReactionEvent_HemoReactionGUID_OccurredAt", table: "HemoReactionEvent", columns: new[] { "HemoReactionGUID", "OccurredAt" });
        migrationBuilder.CreateTable(name: "HemoCapa", columns: table => new
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
            HemoReactionGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
            Kind = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
            OwnerUserGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            OwnerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
            DueAt = table.Column<DateTime>(type: "datetime2", nullable: false),
            Indicator = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
            Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
            Progress = table.Column<int>(type: "int", nullable: false),
            Evidence = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
            SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
            SubmittedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            VerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
            VerifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            VerificationNotes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
            Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_HemoCapa", x=>x.GUID);
            table.ForeignKey("FK_HemoCapa_HemoReaction_HemoReactionGUID", x=>x.HemoReactionGUID, "HemoReaction", "GUID", onDelete: ReferentialAction.Restrict);
            table.ForeignKey("FK_HemoCapa_User_OwnerUserGUID", x=>x.OwnerUserGUID, "User", "GUID", onDelete: ReferentialAction.Restrict);
        });
        migrationBuilder.CreateIndex(name: "IX_HemoCapa_HemoReactionGUID_Status", table: "HemoCapa", columns: new[] { "HemoReactionGUID", "Status" });
        migrationBuilder.CreateIndex(name: "IX_HemoCapa_OwnerUserGUID_Status_DueAt", table: "HemoCapa", columns: new[] { "OwnerUserGUID", "Status", "DueAt" });
        migrationBuilder.CreateTable(name: "HemoReport", columns: table => new
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
            ReportNumber = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
            Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
            FromAt = table.Column<DateTime>(type: "datetime2", nullable: false),
            ToAt = table.Column<DateTime>(type: "datetime2", nullable: false),
            Ward = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
            ProductCode = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
            PreparedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            PreparedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
            SnapshotJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
            ExternalStatus = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
            ExternalReference = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
            ExternalRecordedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            ExternalRecordedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
            Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_HemoReport", x=>x.GUID);
            table.ForeignKey("FK_HemoReport_OrgLocation_OrgLocationGUID", x=>x.OrgLocationGUID, "OrgLocation", "GUID", onDelete: ReferentialAction.Restrict);
        });
        migrationBuilder.CreateIndex(name: "IX_HemoReport_OrgLocationGUID_ReportNumber", table: "HemoReport", columns: new[] { "OrgLocationGUID", "ReportNumber" }, unique: true);
        migrationBuilder.CreateIndex(name: "IX_HemoReport_OrgLocationGUID_PreparedAt", table: "HemoReport", columns: new[] { "OrgLocationGUID", "PreparedAt" });
    }
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("HemoReport");
        migrationBuilder.DropTable("HemoCapa");
        migrationBuilder.DropTable("HemoReactionEvent");
        migrationBuilder.DropTable("HemoReaction");
    }
}
