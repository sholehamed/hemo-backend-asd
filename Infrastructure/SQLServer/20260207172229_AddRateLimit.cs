using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.SQLSERVER
{
    /// <inheritdoc />
    public partial class AddRateLimit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EnableRateLimiting",
                table: "Api",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Limit",
                table: "Api",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "LogRequestBody",
                table: "Api",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "LogRequestHeaders",
                table: "Api",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "LogResponseBody",
                table: "Api",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "LogResponseHeaders",
                table: "Api",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Period",
                table: "Api",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnableRateLimiting",
                table: "Api");

            migrationBuilder.DropColumn(
                name: "Limit",
                table: "Api");

            migrationBuilder.DropColumn(
                name: "LogRequestBody",
                table: "Api");

            migrationBuilder.DropColumn(
                name: "LogRequestHeaders",
                table: "Api");

            migrationBuilder.DropColumn(
                name: "LogResponseBody",
                table: "Api");

            migrationBuilder.DropColumn(
                name: "LogResponseHeaders",
                table: "Api");

            migrationBuilder.DropColumn(
                name: "Period",
                table: "Api");
        }
    }
}
