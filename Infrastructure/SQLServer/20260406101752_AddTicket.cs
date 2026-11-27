using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.SQLSERVER
{
    /// <inheritdoc />
    public partial class AddTicket : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Department",
                columns: table => new
                {
                    GUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Gkey = table.Column<long>(type: "bigint", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ConvertID = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Department", x => x.GUID);
                });

            migrationBuilder.CreateTable(
                name: "Priority",
                columns: table => new
                {
                    GUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Gkey = table.Column<long>(type: "bigint", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ConvertID = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Priority", x => x.GUID);
                });

            migrationBuilder.CreateTable(
                name: "TicketStatus",
                columns: table => new
                {
                    GUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Gkey = table.Column<long>(type: "bigint", nullable: false),
                    Style = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ConvertID = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketStatus", x => x.GUID);
                });

            migrationBuilder.CreateTable(
                name: "Ticket",
                columns: table => new
                {
                    GUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    TicketStatusGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DepartmentGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PriorityGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TicketNumber = table.Column<long>(type: "bigint", nullable: false),
                    UserGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("PK_Ticket", x => x.GUID);
                    table.ForeignKey(
                        name: "FK_Ticket_Department_DepartmentGUID",
                        column: x => x.DepartmentGUID,
                        principalTable: "Department",
                        principalColumn: "GUID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ticket_Priority_PriorityGUID",
                        column: x => x.PriorityGUID,
                        principalTable: "Priority",
                        principalColumn: "GUID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ticket_TicketStatus_TicketStatusGUID",
                        column: x => x.TicketStatusGUID,
                        principalTable: "TicketStatus",
                        principalColumn: "GUID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TicketMessage",
                columns: table => new
                {
                    GUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TicketGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    IsFile = table.Column<bool>(type: "bit", nullable: false),
                    UserGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SendDate = table.Column<DateTime>(type: "datetime2", nullable: false),
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
                    table.PrimaryKey("PK_TicketMessage", x => x.GUID);
                    table.ForeignKey(
                        name: "FK_TicketMessage_Ticket_TicketGUID",
                        column: x => x.TicketGUID,
                        principalTable: "Ticket",
                        principalColumn: "GUID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Department_Gkey",
                table: "Department",
                column: "Gkey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Priority_Gkey",
                table: "Priority",
                column: "Gkey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_DeletedBy",
                table: "Ticket",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_DepartmentGUID",
                table: "Ticket",
                column: "DepartmentGUID");

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_PriorityGUID",
                table: "Ticket",
                column: "PriorityGUID");

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_TicketStatusGUID",
                table: "Ticket",
                column: "TicketStatusGUID");

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_UserGUID",
                table: "Ticket",
                column: "UserGUID");

            migrationBuilder.CreateIndex(
                name: "IX_TicketMessage_DeletedBy",
                table: "TicketMessage",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_TicketMessage_TicketGUID",
                table: "TicketMessage",
                column: "TicketGUID");

            migrationBuilder.CreateIndex(
                name: "IX_TicketMessage_UserGUID",
                table: "TicketMessage",
                column: "UserGUID");

            migrationBuilder.CreateIndex(
                name: "IX_TicketStatus_Gkey",
                table: "TicketStatus",
                column: "Gkey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TicketMessage");

            migrationBuilder.DropTable(
                name: "Ticket");

            migrationBuilder.DropTable(
                name: "Department");

            migrationBuilder.DropTable(
                name: "Priority");

            migrationBuilder.DropTable(
                name: "TicketStatus");
        }
    }
}
