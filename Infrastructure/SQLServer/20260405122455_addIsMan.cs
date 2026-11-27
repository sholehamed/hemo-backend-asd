using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.SQLSERVER
{
    /// <inheritdoc />
    public partial class addIsMan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsMan",
                table: "User",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsMan",
                table: "User");
        }
    }
}
