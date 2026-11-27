using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.SQLSERVER
{
    /// <inheritdoc />
    public partial class AddNationalcode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Nationalcode",
                table: "User",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Nationalcode",
                table: "User");
        }
    }
}
