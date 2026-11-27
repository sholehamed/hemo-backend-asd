using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.SQLServer
{
    /// <inheritdoc />
    public partial class AddShoppingCart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ShoppingCartStatus",
                columns: table => new
                {
                    GUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Gkey = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShoppingCartStatus", x => x.GUID);
                });

            migrationBuilder.CreateTable(
                name: "ShoppingCart",
                columns: table => new
                {
                    GUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ShoppingCartStatusGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShoppingCart", x => x.GUID);
                    table.ForeignKey(
                        name: "FK_ShoppingCart_ShoppingCartStatus_ShoppingCartStatusGUID",
                        column: x => x.ShoppingCartStatusGUID,
                        principalTable: "ShoppingCartStatus",
                        principalColumn: "GUID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShoppingCart_User_UserGUID",
                        column: x => x.UserGUID,
                        principalTable: "User",
                        principalColumn: "GUID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShoppingCartProduct",
                columns: table => new
                {
                    GUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PackageGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ShoppingCartGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrgLocationGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShoppingCartProduct", x => x.GUID);
                    table.ForeignKey(
                        name: "FK_ShoppingCartProduct_OrgLocation_OrgLocationGUID",
                        column: x => x.OrgLocationGUID,
                        principalTable: "OrgLocation",
                        principalColumn: "GUID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShoppingCartProduct_Package_PackageGUID",
                        column: x => x.PackageGUID,
                        principalTable: "Package",
                        principalColumn: "GUID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShoppingCartProduct_ShoppingCart_ShoppingCartGUID",
                        column: x => x.ShoppingCartGUID,
                        principalTable: "ShoppingCart",
                        principalColumn: "GUID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingCart_ShoppingCartStatusGUID",
                table: "ShoppingCart",
                column: "ShoppingCartStatusGUID");

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingCart_UserGUID",
                table: "ShoppingCart",
                column: "UserGUID");

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingCartProduct_OrgLocationGUID",
                table: "ShoppingCartProduct",
                column: "OrgLocationGUID");

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingCartProduct_PackageGUID",
                table: "ShoppingCartProduct",
                column: "PackageGUID");

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingCartProduct_ShoppingCartGUID",
                table: "ShoppingCartProduct",
                column: "ShoppingCartGUID");

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingCartStatus_Gkey",
                table: "ShoppingCartStatus",
                column: "Gkey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShoppingCartProduct");

            migrationBuilder.DropTable(
                name: "ShoppingCart");

            migrationBuilder.DropTable(
                name: "ShoppingCartStatus");
        }
    }
}
