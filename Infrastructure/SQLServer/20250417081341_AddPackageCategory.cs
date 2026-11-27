using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.SQLServer
{
    /// <inheritdoc />
    public partial class AddPackageCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PackageCategoryGUID",
                table: "Package",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "PackageCategory",
                columns: table => new
                {
                    GUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
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
                    table.PrimaryKey("PK_PackageCategory", x => x.GUID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Package_PackageCategoryGUID",
                table: "Package",
                column: "PackageCategoryGUID");

            migrationBuilder.CreateIndex(
                name: "IX_PackageCategory_DeletedBy",
                table: "PackageCategory",
                column: "DeletedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_Package_PackageCategory_PackageCategoryGUID",
                table: "Package",
                column: "PackageCategoryGUID",
                principalTable: "PackageCategory",
                principalColumn: "GUID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Package_PackageCategory_PackageCategoryGUID",
                table: "Package");

            migrationBuilder.DropTable(
                name: "PackageCategory");

            migrationBuilder.DropIndex(
                name: "IX_Package_PackageCategoryGUID",
                table: "Package");

            migrationBuilder.DropColumn(
                name: "PackageCategoryGUID",
                table: "Package");
        }
    }
}
