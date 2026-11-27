using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.SQLSERVER
{
    /// <inheritdoc />
    public partial class UpdateShopCard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "Price",
                table: "ShoppingCartProduct",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DiscountAmount",
                table: "ShoppingCart",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DiscountGUID",
                table: "ShoppingCart",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "FinalAmount",
                table: "ShoppingCart",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "OrderAmount",
                table: "ShoppingCart",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "Tax",
                table: "ShoppingCart",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Discount",
                columns: table => new
                {
                    GUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Percentage = table.Column<bool>(type: "bit", nullable: false),
                    Value = table.Column<int>(type: "int", nullable: false),
                    MaxDiscountAmount = table.Column<int>(type: "int", nullable: true),
                    MinimumOrderAmount = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    UsageLimit = table.Column<int>(type: "int", nullable: false),
                    UsageLimitPerUser = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ConvertID = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
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
                    table.PrimaryKey("PK_Discount", x => x.GUID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingCart_DiscountGUID",
                table: "ShoppingCart",
                column: "DiscountGUID");

            migrationBuilder.CreateIndex(
                name: "IX_Discount_Code",
                table: "Discount",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Discount_DeletedBy",
                table: "Discount",
                column: "DeletedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_ShoppingCart_Discount_DiscountGUID",
                table: "ShoppingCart",
                column: "DiscountGUID",
                principalTable: "Discount",
                principalColumn: "GUID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShoppingCart_Discount_DiscountGUID",
                table: "ShoppingCart");

            migrationBuilder.DropTable(
                name: "Discount");

            migrationBuilder.DropIndex(
                name: "IX_ShoppingCart_DiscountGUID",
                table: "ShoppingCart");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "ShoppingCartProduct");

            migrationBuilder.DropColumn(
                name: "DiscountAmount",
                table: "ShoppingCart");

            migrationBuilder.DropColumn(
                name: "DiscountGUID",
                table: "ShoppingCart");

            migrationBuilder.DropColumn(
                name: "FinalAmount",
                table: "ShoppingCart");

            migrationBuilder.DropColumn(
                name: "OrderAmount",
                table: "ShoppingCart");

            migrationBuilder.DropColumn(
                name: "Tax",
                table: "ShoppingCart");
        }
    }
}
