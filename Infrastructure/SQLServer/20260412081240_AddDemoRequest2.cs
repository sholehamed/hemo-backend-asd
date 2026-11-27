using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.SQLSERVER
{
    /// <inheritdoc />
    public partial class AddDemoRequest2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DemoRequestGUID",
                table: "OrglocationPackage",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrglocationPackage_DemoRequestGUID",
                table: "OrglocationPackage",
                column: "DemoRequestGUID");

            migrationBuilder.AddForeignKey(
                name: "FK_OrglocationPackage_DemoRequest_DemoRequestGUID",
                table: "OrglocationPackage",
                column: "DemoRequestGUID",
                principalTable: "DemoRequest",
                principalColumn: "GUID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrglocationPackage_DemoRequest_DemoRequestGUID",
                table: "OrglocationPackage");

            migrationBuilder.DropIndex(
                name: "IX_OrglocationPackage_DemoRequestGUID",
                table: "OrglocationPackage");

            migrationBuilder.DropColumn(
                name: "DemoRequestGUID",
                table: "OrglocationPackage");
        }
    }
}
