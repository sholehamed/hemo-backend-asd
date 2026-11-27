using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.SQLServer
{
    /// <inheritdoc />
    public partial class AddConvertID : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConvertID",
                table: "UserRole",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConvertID",
                table: "UserInvite",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConvertID",
                table: "User",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConvertID",
                table: "SubSystem",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConvertID",
                table: "sso",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConvertID",
                table: "ShoppingCartStatus",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConvertID",
                table: "ShoppingCartProduct",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConvertID",
                table: "ShoppingCart",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConvertID",
                table: "RoleAccess",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConvertID",
                table: "Role",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConvertID",
                table: "PackageCategory",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConvertID",
                table: "Package",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConvertID",
                table: "OrgType",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConvertID",
                table: "OrgLocationType",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConvertID",
                table: "OrglocationPackage",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConvertID",
                table: "OrgLocation",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConvertID",
                table: "MenuApi",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConvertID",
                table: "Menu",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConvertID",
                table: "Api",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConvertID",
                table: "UserRole");

            migrationBuilder.DropColumn(
                name: "ConvertID",
                table: "UserInvite");

            migrationBuilder.DropColumn(
                name: "ConvertID",
                table: "User");

            migrationBuilder.DropColumn(
                name: "ConvertID",
                table: "SubSystem");

            migrationBuilder.DropColumn(
                name: "ConvertID",
                table: "sso");

            migrationBuilder.DropColumn(
                name: "ConvertID",
                table: "ShoppingCartStatus");

            migrationBuilder.DropColumn(
                name: "ConvertID",
                table: "ShoppingCartProduct");

            migrationBuilder.DropColumn(
                name: "ConvertID",
                table: "ShoppingCart");

            migrationBuilder.DropColumn(
                name: "ConvertID",
                table: "RoleAccess");

            migrationBuilder.DropColumn(
                name: "ConvertID",
                table: "Role");

            migrationBuilder.DropColumn(
                name: "ConvertID",
                table: "PackageCategory");

            migrationBuilder.DropColumn(
                name: "ConvertID",
                table: "Package");

            migrationBuilder.DropColumn(
                name: "ConvertID",
                table: "OrgType");

            migrationBuilder.DropColumn(
                name: "ConvertID",
                table: "OrgLocationType");

            migrationBuilder.DropColumn(
                name: "ConvertID",
                table: "OrglocationPackage");

            migrationBuilder.DropColumn(
                name: "ConvertID",
                table: "OrgLocation");

            migrationBuilder.DropColumn(
                name: "ConvertID",
                table: "MenuApi");

            migrationBuilder.DropColumn(
                name: "ConvertID",
                table: "Menu");

            migrationBuilder.DropColumn(
                name: "ConvertID",
                table: "Api");
        }
    }
}
