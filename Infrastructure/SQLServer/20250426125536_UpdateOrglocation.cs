using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.SQLServer
{
    /// <inheritdoc />
    public partial class UpdateOrglocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OrgLocationTypeGUID",
                table: "OrgLocation",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrgLocationTypeGUIO",
                table: "OrgLocation",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserGUID",
                table: "OrgLocation",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Role_OrgLocationTypeGUIO",
                table: "Role",
                column: "OrgLocationTypeGUIO");

            migrationBuilder.CreateIndex(
                name: "IX_OrgLocation_OrgLocationTypeGUID",
                table: "OrgLocation",
                column: "OrgLocationTypeGUID");

            migrationBuilder.CreateIndex(
                name: "IX_OrgLocation_OrgLocationTypeGUIO",
                table: "OrgLocation",
                column: "OrgLocationTypeGUIO");

            migrationBuilder.CreateIndex(
                name: "IX_OrgLocation_UserGUID",
                table: "OrgLocation",
                column: "UserGUID");

            migrationBuilder.AddForeignKey(
                name: "FK_OrgLocation_OrgLocationType_OrgLocationTypeGUID",
                table: "OrgLocation",
                column: "OrgLocationTypeGUID",
                principalTable: "OrgLocationType",
                principalColumn: "GUID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrgLocation_User_UserGUID",
                table: "OrgLocation",
                column: "UserGUID",
                principalTable: "User",
                principalColumn: "GUID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrgLocation_OrgLocationType_OrgLocationTypeGUID",
                table: "OrgLocation");

            migrationBuilder.DropForeignKey(
                name: "FK_OrgLocation_User_UserGUID",
                table: "OrgLocation");

            migrationBuilder.DropIndex(
                name: "IX_Role_OrgLocationTypeGUIO",
                table: "Role");

            migrationBuilder.DropIndex(
                name: "IX_OrgLocation_OrgLocationTypeGUID",
                table: "OrgLocation");

            migrationBuilder.DropIndex(
                name: "IX_OrgLocation_OrgLocationTypeGUIO",
                table: "OrgLocation");

            migrationBuilder.DropIndex(
                name: "IX_OrgLocation_UserGUID",
                table: "OrgLocation");

            migrationBuilder.DropColumn(
                name: "OrgLocationTypeGUID",
                table: "OrgLocation");

            migrationBuilder.DropColumn(
                name: "OrgLocationTypeGUIO",
                table: "OrgLocation");

            migrationBuilder.DropColumn(
                name: "UserGUID",
                table: "OrgLocation");
        }
    }
}
