using Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
namespace Infrastructure.SQLServer;
[DbContext(typeof(ApplicationDbContext))]
[Migration("20261127220000_AddHemoTransfusion")]
public partial class AddHemoTransfusion : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(name: "BloodTransfer", columns: table => new
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
            BankBloodUnitGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            BloodRequestGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            PatientAdmissionGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            BankCompatibilityTestGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            TransferNumber = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
            PatientName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
            HisNumber = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
            Ward = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
            WristbandCode = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
            UnitCode = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
            ProductCode = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
            BloodGroup = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
            RequestVersion = table.Column<byte[]>(type: "varbinary(8)", maxLength: 8, nullable: true),
            AdmissionVersion = table.Column<byte[]>(type: "varbinary(8)", maxLength: 8, nullable: true),
            Status = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
            DispatchedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
            DispatchedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            CarrierName = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
            CarrierCode = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
            ContainerCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
            DispatchTemperature = table.Column<decimal>(type: "decimal(9,2)", precision: 9, scale: 2, nullable: false),
            TransportIndicator = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
            PackCount = table.Column<int>(type: "int", nullable: true),
            ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
            ReceivedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            VerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
            VerifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            WitnessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
            WitnessedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
            StartedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            EndedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
            EndedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            SuspectedReaction = table.Column<bool>(type: "bit", nullable: false),
            ReturnedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
            ReturnAcceptedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
            Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_BloodTransfer", x=>x.GUID);
            table.ForeignKey("FK_BloodTransfer_OrgLocation_OrgLocationGUID", x=>x.OrgLocationGUID, "OrgLocation", "GUID", onDelete: ReferentialAction.Restrict);
            table.ForeignKey("FK_BloodTransfer_BankBloodUnit_BankBloodUnitGUID", x=>x.BankBloodUnitGUID, "BankBloodUnit", "GUID", onDelete: ReferentialAction.Restrict);
            table.ForeignKey("FK_BloodTransfer_BloodRequest_BloodRequestGUID", x=>x.BloodRequestGUID, "BloodRequest", "GUID", onDelete: ReferentialAction.Restrict);
            table.ForeignKey("FK_BloodTransfer_PatientAdmission_PatientAdmissionGUID", x=>x.PatientAdmissionGUID, "PatientAdmission", "GUID", onDelete: ReferentialAction.Restrict);
            table.ForeignKey("FK_BloodTransfer_BankCompatibilityTest_BankCompatibilityTestGUID", x=>x.BankCompatibilityTestGUID, "BankCompatibilityTest", "GUID", onDelete: ReferentialAction.Restrict);
        });
        migrationBuilder.CreateIndex(name: "IX_BloodTransfer_BankBloodUnitGUID", table: "BloodTransfer", column: "BankBloodUnitGUID", unique: true);
        migrationBuilder.CreateIndex(name: "IX_BloodTransfer_OrgLocationGUID_TransferNumber", table: "BloodTransfer", columns: new[] { "OrgLocationGUID", "TransferNumber" }, unique: true);
        migrationBuilder.CreateIndex(name: "IX_BloodTransfer_OrgLocationGUID_Status_DispatchedAt", table: "BloodTransfer", columns: new[] { "OrgLocationGUID", "Status", "DispatchedAt" });
        migrationBuilder.CreateIndex(name: "IX_BloodTransfer_BloodRequestGUID", table: "BloodTransfer", column: "BloodRequestGUID");
        migrationBuilder.CreateIndex(name: "IX_BloodTransfer_PatientAdmissionGUID", table: "BloodTransfer", column: "PatientAdmissionGUID");
        migrationBuilder.CreateIndex(name: "IX_BloodTransfer_BankCompatibilityTestGUID", table: "BloodTransfer", column: "BankCompatibilityTestGUID");
        migrationBuilder.CreateTable(name: "BloodTransferEvent", columns: table => new
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
            BloodTransferGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            ActorUserGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            ActorRoleGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            Action = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
            PreviousStatus = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
            NewStatus = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
            OccurredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
            Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
            EvidenceJson = table.Column<string>(type: "nvarchar(max)", nullable: true)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_BloodTransferEvent", x=>x.GUID);
            table.ForeignKey("FK_BloodTransferEvent_BloodTransfer_BloodTransferGUID", x=>x.BloodTransferGUID, "BloodTransfer", "GUID", onDelete: ReferentialAction.Restrict);
        });
        migrationBuilder.CreateIndex(name: "IX_BloodTransferEvent_BloodTransferGUID_OccurredAt", table: "BloodTransferEvent", columns: new[] { "BloodTransferGUID", "OccurredAt" });
        migrationBuilder.CreateTable(name: "BloodTransferVital", columns: table => new
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
            BloodTransferGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            ActorUserGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            ActorRoleGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            Phase = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
            MeasuredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
            RecordedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
            Systolic = table.Column<int>(type: "int", nullable: false),
            Diastolic = table.Column<int>(type: "int", nullable: false),
            Pulse = table.Column<int>(type: "int", nullable: false),
            Temperature = table.Column<decimal>(type: "decimal(9,2)", precision: 9, scale: 2, nullable: false),
            Spo2 = table.Column<int>(type: "int", nullable: false),
            Respiration = table.Column<int>(type: "int", nullable: false),
            UrineMl = table.Column<decimal>(type: "decimal(9,2)", precision: 9, scale: 2, nullable: true),
            Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_BloodTransferVital", x=>x.GUID);
            table.ForeignKey("FK_BloodTransferVital_BloodTransfer_BloodTransferGUID", x=>x.BloodTransferGUID, "BloodTransfer", "GUID", onDelete: ReferentialAction.Restrict);
        });
        migrationBuilder.CreateIndex(name: "IX_BloodTransferVital_BloodTransferGUID_MeasuredAt", table: "BloodTransferVital", columns: new[] { "BloodTransferGUID", "MeasuredAt" });
        migrationBuilder.CreateIndex(name: "IX_BloodTransferVital_BloodTransferGUID_Phase", table: "BloodTransferVital", columns: new[] { "BloodTransferGUID", "Phase" });
    }
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("BloodTransferVital");
        migrationBuilder.DropTable("BloodTransferEvent");
        migrationBuilder.DropTable("BloodTransfer");
    }
}
