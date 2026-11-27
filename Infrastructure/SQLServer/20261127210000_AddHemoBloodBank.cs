using Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
namespace Infrastructure.SQLServer;
[DbContext(typeof(ApplicationDbContext))]
[Migration("20261127210000_AddHemoBloodBank")]
public partial class AddHemoBloodBank : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(name: "BankBloodUnit", columns: table => new
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
            UnitCode = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
            ProductCode = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
            BloodGroup = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
            BatchNumber = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
            SupplySource = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
            EquipmentCode = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
            ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
            ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
            Status = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
            InspectionAt = table.Column<DateTime>(type: "datetime2", nullable: true),
            BloodRequestGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            ThawStartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
            ThawCompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
            Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_BankBloodUnit", x=>x.GUID);
            table.ForeignKey("FK_BankBloodUnit_OrgLocation_OrgLocationGUID", x=>x.OrgLocationGUID, "OrgLocation", "GUID", onDelete: ReferentialAction.Restrict);
            table.ForeignKey("FK_BankBloodUnit_BloodRequest_BloodRequestGUID", x=>x.BloodRequestGUID, "BloodRequest", "GUID", onDelete: ReferentialAction.Restrict);
        });
        migrationBuilder.CreateIndex(name: "IX_BankBloodUnit_OrgLocationGUID_UnitCode", table: "BankBloodUnit", columns: new[] { "OrgLocationGUID", "UnitCode" }, unique: true);
        migrationBuilder.CreateIndex(name: "IX_BankBloodUnit_OrgLocationGUID_Status_ExpiresAt", table: "BankBloodUnit", columns: new[] { "OrgLocationGUID", "Status", "ExpiresAt" });
        migrationBuilder.CreateIndex(name: "IX_BankBloodUnit_BloodRequestGUID_Status", table: "BankBloodUnit", columns: new[] { "BloodRequestGUID", "Status" });
        migrationBuilder.CreateIndex(name: "IX_BankBloodUnit_OrgLocationGUID_EquipmentCode", table: "BankBloodUnit", columns: new[] { "OrgLocationGUID", "EquipmentCode" });
        migrationBuilder.CreateTable(name: "BankCompatibilityTest", columns: table => new
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
            BankBloodUnitGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            BloodRequestGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            SubmissionAttempt = table.Column<int>(type: "int", nullable: false),
            RequestVersion = table.Column<byte[]>(type: "varbinary(8)", maxLength: 8, nullable: true),
            AdmissionVersion = table.Column<byte[]>(type: "varbinary(8)", maxLength: 8, nullable: true),
            ActorUserGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            ActorRoleGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            RecordedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
            SampleReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
            SecondSample = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
            LabelGroup = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
            Method = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
            ImmediateSpin = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
            Phase37 = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
            Ahg = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
            Dat = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
            AntibodyScreen = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
            ReportReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
            Conclusion = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
            Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_BankCompatibilityTest", x=>x.GUID);
            table.ForeignKey("FK_BankCompatibilityTest_BankBloodUnit_BankBloodUnitGUID", x=>x.BankBloodUnitGUID, "BankBloodUnit", "GUID", onDelete: ReferentialAction.Restrict);
            table.ForeignKey("FK_BankCompatibilityTest_BloodRequest_BloodRequestGUID", x=>x.BloodRequestGUID, "BloodRequest", "GUID", onDelete: ReferentialAction.Restrict);
        });
        migrationBuilder.CreateIndex(name: "IX_BankCompatibilityTest_BankBloodUnitGUID_BloodRequestGUID_RecordedAt", table: "BankCompatibilityTest", columns: new[] { "BankBloodUnitGUID", "BloodRequestGUID", "RecordedAt" });
        migrationBuilder.CreateIndex(name: "IX_BankCompatibilityTest_BloodRequestGUID", table: "BankCompatibilityTest", column: "BloodRequestGUID");
        migrationBuilder.CreateTable(name: "BankUnitEvent", columns: table => new
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
            BankBloodUnitGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            BloodRequestGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            ActorUserGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            ActorRoleGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            Action = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
            PreviousStatus = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
            NewStatus = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
            OccurredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
            Reason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
            Method = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
            Temperature = table.Column<decimal>(type: "decimal(7,2)", precision: 7, scale: 2, nullable: true),
            EffectiveExpiry = table.Column<DateTime>(type: "datetime2", nullable: true)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_BankUnitEvent", x=>x.GUID);
            table.ForeignKey("FK_BankUnitEvent_BankBloodUnit_BankBloodUnitGUID", x=>x.BankBloodUnitGUID, "BankBloodUnit", "GUID", onDelete: ReferentialAction.Restrict);
            table.ForeignKey("FK_BankUnitEvent_BloodRequest_BloodRequestGUID", x=>x.BloodRequestGUID, "BloodRequest", "GUID", onDelete: ReferentialAction.Restrict);
        });
        migrationBuilder.CreateIndex(name: "IX_BankUnitEvent_BankBloodUnitGUID_OccurredAt", table: "BankUnitEvent", columns: new[] { "BankBloodUnitGUID", "OccurredAt" });
        migrationBuilder.CreateIndex(name: "IX_BankUnitEvent_BloodRequestGUID", table: "BankUnitEvent", column: "BloodRequestGUID");
        migrationBuilder.CreateTable(name: "BankTemperatureLog", columns: table => new
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
            EquipmentCode = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
            MeasuredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
            RecordedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
            Temperature = table.Column<decimal>(type: "decimal(7,2)", precision: 7, scale: 2, nullable: false),
            Assessment = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
            Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
            ActorUserGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            ActorRoleGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_BankTemperatureLog", x=>x.GUID);
            table.ForeignKey("FK_BankTemperatureLog_OrgLocation_OrgLocationGUID", x=>x.OrgLocationGUID, "OrgLocation", "GUID", onDelete: ReferentialAction.Restrict);
        });
        migrationBuilder.CreateIndex(name: "IX_BankTemperatureLog_OrgLocationGUID_EquipmentCode_MeasuredAt", table: "BankTemperatureLog", columns: new[] { "OrgLocationGUID", "EquipmentCode", "MeasuredAt" });
        migrationBuilder.CreateIndex(name: "IX_BankTemperatureLog_OrgLocationGUID_RecordedAt", table: "BankTemperatureLog", columns: new[] { "OrgLocationGUID", "RecordedAt" });
    }
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("BankTemperatureLog");
        migrationBuilder.DropTable("BankUnitEvent");
        migrationBuilder.DropTable("BankCompatibilityTest");
        migrationBuilder.DropTable("BankBloodUnit");
    }
}
