using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.SQLSERVER
{
    /// <inheritdoc />
    public partial class PackageMenuRelationAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Package_MainMenuGuid",
                table: "Package",
                column: "MainMenuGuid");

            migrationBuilder.AddForeignKey(
                name: "FK_Package_Menu_MainMenuGuid",
                table: "Package",
                column: "MainMenuGuid",
                principalTable: "Menu",
                principalColumn: "GUID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Package_Menu_MainMenuGuid",
                table: "Package");

            migrationBuilder.DropIndex(
                name: "IX_Package_MainMenuGuid",
                table: "Package");
        }
    }
}
