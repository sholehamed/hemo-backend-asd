using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.SQLServer
{
    /// <inheritdoc />
    public partial class UpdateOrglocationTypeGUID : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Role_OrgLocationTypeGUIO",
                table: "Role");

            migrationBuilder.DropColumn(
                name: "OrgLocationTypeGUIO",
                table: "Role");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OrgLocationTypeGUIO",
                table: "Role",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Role_OrgLocationTypeGUIO",
                table: "Role",
                column: "OrgLocationTypeGUIO");
        }
    }
}
