using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.SQLServer
{
    /// <inheritdoc />
    public partial class AddOrgType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OrgLocationTypeGUID",
                table: "Role",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrgLocationTypeGUIO",
                table: "Role",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "OrgLocationType",
                columns: table => new
                {
                    GUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrgLocationType", x => x.GUID);
                });

            migrationBuilder.CreateTable(
                name: "OrgType",
                columns: table => new
                {
                    GUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrgLocationTypeGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrgLocationGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrgType", x => x.GUID);
                    table.ForeignKey(
                        name: "FK_OrgType_OrgLocationType_OrgLocationTypeGUID",
                        column: x => x.OrgLocationTypeGUID,
                        principalTable: "OrgLocationType",
                        principalColumn: "GUID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrgType_OrgLocation_OrgLocationGUID",
                        column: x => x.OrgLocationGUID,
                        principalTable: "OrgLocation",
                        principalColumn: "GUID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Role_OrgLocationTypeGUID",
                table: "Role",
                column: "OrgLocationTypeGUID");

            migrationBuilder.CreateIndex(
                name: "IX_OrgType_OrgLocationGUID",
                table: "OrgType",
                column: "OrgLocationGUID");

            migrationBuilder.CreateIndex(
                name: "IX_OrgType_OrgLocationTypeGUID",
                table: "OrgType",
                column: "OrgLocationTypeGUID");

            migrationBuilder.AddForeignKey(
                name: "FK_Role_OrgLocationType_OrgLocationTypeGUID",
                table: "Role",
                column: "OrgLocationTypeGUID",
                principalTable: "OrgLocationType",
                principalColumn: "GUID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Role_OrgLocationType_OrgLocationTypeGUID",
                table: "Role");

            migrationBuilder.DropTable(
                name: "OrgType");

            migrationBuilder.DropTable(
                name: "OrgLocationType");

            migrationBuilder.DropIndex(
                name: "IX_Role_OrgLocationTypeGUID",
                table: "Role");

            migrationBuilder.DropColumn(
                name: "OrgLocationTypeGUID",
                table: "Role");

            migrationBuilder.DropColumn(
                name: "OrgLocationTypeGUIO",
                table: "Role");
        }
    }
}
