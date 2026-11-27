using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.SQLSERVER
{
    /// <inheritdoc />
    public partial class AddSystemSetting2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SystemSetting",
                columns: table => new
                {
                    GUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TokenExpireSeconds = table.Column<int>(type: "int", nullable: false),
                    RefreshTokenExpireSeconds = table.Column<int>(type: "int", nullable: false),
                    LoginAttempts = table.Column<int>(type: "int", nullable: false),
                    LockUserSeconds = table.Column<int>(type: "int", nullable: false),
                    LoginApiGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    IsDelete = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSetting", x => x.GUID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SystemSetting_DeletedBy",
                table: "SystemSetting",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_SystemSetting_LoginApiGUID",
                table: "SystemSetting",
                column: "LoginApiGUID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SystemSetting");
        }
    }
}
