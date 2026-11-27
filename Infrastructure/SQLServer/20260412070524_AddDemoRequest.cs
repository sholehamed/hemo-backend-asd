using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.SQLSERVER
{
    /// <inheritdoc />
    public partial class AddDemoRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DemoRequest",
                columns: table => new
                {
                    GUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PackageGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrgLocationGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Accepted = table.Column<bool>(type: "bit", nullable: true),
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
                    table.PrimaryKey("PK_DemoRequest", x => x.GUID);
                    table.ForeignKey(
                        name: "FK_DemoRequest_OrgLocation_OrgLocationGUID",
                        column: x => x.OrgLocationGUID,
                        principalTable: "OrgLocation",
                        principalColumn: "GUID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DemoRequest_Package_PackageGUID",
                        column: x => x.PackageGUID,
                        principalTable: "Package",
                        principalColumn: "GUID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DemoRequest_User_UserGUID",
                        column: x => x.UserGUID,
                        principalTable: "User",
                        principalColumn: "GUID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DemoRequest_DeletedBy",
                table: "DemoRequest",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_DemoRequest_OrgLocationGUID",
                table: "DemoRequest",
                column: "OrgLocationGUID");

            migrationBuilder.CreateIndex(
                name: "IX_DemoRequest_PackageGUID",
                table: "DemoRequest",
                column: "PackageGUID");

            migrationBuilder.CreateIndex(
                name: "IX_DemoRequest_UserGUID",
                table: "DemoRequest",
                column: "UserGUID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DemoRequest");
        }
    }
}
