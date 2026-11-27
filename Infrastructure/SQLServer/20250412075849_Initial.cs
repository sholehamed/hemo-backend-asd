using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.SQLServer
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Menu",
                columns: table => new
                {
                    GUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PMenuGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    label = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    icon = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    routerLink = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Gorder = table.Column<int>(type: "int", nullable: false),
                    ShowInMenu = table.Column<bool>(type: "bit", nullable: false),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false),
                    Path = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    Gkey = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Menu", x => x.GUID);
                    table.ForeignKey(
                        name: "FK_Menu_Menu_PMenuGUID",
                        column: x => x.PMenuGUID,
                        principalTable: "Menu",
                        principalColumn: "GUID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrgLocation",
                columns: table => new
                {
                    GUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    POrgLocationGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Tell = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Fax = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Url = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Gkey = table.Column<int>(type: "int", nullable: false),
                    Path = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
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
                    table.PrimaryKey("PK_OrgLocation", x => x.GUID);
                    table.ForeignKey(
                        name: "FK_OrgLocation_OrgLocation_POrgLocationGUID",
                        column: x => x.POrgLocationGUID,
                        principalTable: "OrgLocation",
                        principalColumn: "GUID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sso",
                columns: table => new
                {
                    GUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PackageGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrgLocationGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IPAddress = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    UserAgent = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ExpireDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Sign = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sso", x => x.GUID);
                });

            migrationBuilder.CreateTable(
                name: "SubSystem",
                columns: table => new
                {
                    GUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Host = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Port = table.Column<int>(type: "int", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubSystem", x => x.GUID);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    GUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Password = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Mobile = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
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
                    table.PrimaryKey("PK_User", x => x.GUID);
                });

            migrationBuilder.CreateTable(
                name: "Role",
                columns: table => new
                {
                    GUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PRoleGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    OrgLocationGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Path = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Gkey = table.Column<int>(type: "int", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
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
                    table.PrimaryKey("PK_Role", x => x.GUID);
                    table.ForeignKey(
                        name: "FK_Role_OrgLocation_OrgLocationGUID",
                        column: x => x.OrgLocationGUID,
                        principalTable: "OrgLocation",
                        principalColumn: "GUID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Role_Role_PRoleGUID",
                        column: x => x.PRoleGUID,
                        principalTable: "Role",
                        principalColumn: "GUID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Api",
                columns: table => new
                {
                    GUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DownstreamPathTemplate = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    UpstreamPathTemplate = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    SubSystemGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    APITypeName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
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
                    table.PrimaryKey("PK_Api", x => x.GUID);
                    table.ForeignKey(
                        name: "FK_Api_SubSystem_SubSystemGUID",
                        column: x => x.SubSystemGUID,
                        principalTable: "SubSystem",
                        principalColumn: "GUID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserInvite",
                columns: table => new
                {
                    GUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrgLocationGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserAccepted = table.Column<bool>(type: "bit", nullable: true),
                    OrgLocationAccepted = table.Column<bool>(type: "bit", nullable: true),
                    UserInfo = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
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
                    table.PrimaryKey("PK_UserInvite", x => x.GUID);
                    table.ForeignKey(
                        name: "FK_UserInvite_OrgLocation_OrgLocationGUID",
                        column: x => x.OrgLocationGUID,
                        principalTable: "OrgLocation",
                        principalColumn: "GUID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserInvite_User_UserGUID",
                        column: x => x.UserGUID,
                        principalTable: "User",
                        principalColumn: "GUID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Package",
                columns: table => new
                {
                    GUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Price = table.Column<long>(type: "bigint", nullable: false),
                    Link = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    RoleGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
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
                    table.PrimaryKey("PK_Package", x => x.GUID);
                    table.ForeignKey(
                        name: "FK_Package_Role_RoleGUID",
                        column: x => x.RoleGUID,
                        principalTable: "Role",
                        principalColumn: "GUID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserRole",
                columns: table => new
                {
                    GUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrgLocationGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExpireDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Gkey = table.Column<long>(type: "bigint", nullable: false),
                    Sign = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
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
                    table.PrimaryKey("PK_UserRole", x => x.GUID);
                    table.ForeignKey(
                        name: "FK_UserRole_OrgLocation_OrgLocationGUID",
                        column: x => x.OrgLocationGUID,
                        principalTable: "OrgLocation",
                        principalColumn: "GUID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserRole_Role_RoleGUID",
                        column: x => x.RoleGUID,
                        principalTable: "Role",
                        principalColumn: "GUID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserRole_User_UserGUID",
                        column: x => x.UserGUID,
                        principalTable: "User",
                        principalColumn: "GUID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MenuApi",
                columns: table => new
                {
                    GUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MenuGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApiGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuApi", x => x.GUID);
                    table.ForeignKey(
                        name: "FK_MenuApi_Api_ApiGUID",
                        column: x => x.ApiGUID,
                        principalTable: "Api",
                        principalColumn: "GUID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MenuApi_Menu_MenuGUID",
                        column: x => x.MenuGUID,
                        principalTable: "Menu",
                        principalColumn: "GUID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrglocationPackage",
                columns: table => new
                {
                    GUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrgLocationGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PackageGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
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
                    table.PrimaryKey("PK_OrglocationPackage", x => x.GUID);
                    table.ForeignKey(
                        name: "FK_OrglocationPackage_OrgLocation_OrgLocationGUID",
                        column: x => x.OrgLocationGUID,
                        principalTable: "OrgLocation",
                        principalColumn: "GUID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrglocationPackage_Package_PackageGUID",
                        column: x => x.PackageGUID,
                        principalTable: "Package",
                        principalColumn: "GUID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RoleAccess",
                columns: table => new
                {
                    GUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MenuGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApiGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PRoleAccessGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OrglocationPackageGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Path = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    Gkey = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
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
                    table.PrimaryKey("PK_RoleAccess", x => x.GUID);
                    table.ForeignKey(
                        name: "FK_RoleAccess_Api_ApiGUID",
                        column: x => x.ApiGUID,
                        principalTable: "Api",
                        principalColumn: "GUID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RoleAccess_Menu_MenuGUID",
                        column: x => x.MenuGUID,
                        principalTable: "Menu",
                        principalColumn: "GUID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RoleAccess_OrglocationPackage_OrglocationPackageGUID",
                        column: x => x.OrglocationPackageGUID,
                        principalTable: "OrglocationPackage",
                        principalColumn: "GUID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RoleAccess_Role_RoleGUID",
                        column: x => x.RoleGUID,
                        principalTable: "Role",
                        principalColumn: "GUID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Api_DeletedBy",
                table: "Api",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Api_SubSystemGUID",
                table: "Api",
                column: "SubSystemGUID");

            migrationBuilder.CreateIndex(
                name: "IX_Menu_Gkey",
                table: "Menu",
                column: "Gkey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Menu_Level",
                table: "Menu",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_Menu_Path",
                table: "Menu",
                column: "Path",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Menu_PMenuGUID",
                table: "Menu",
                column: "PMenuGUID");

            migrationBuilder.CreateIndex(
                name: "IX_Menu_routerLink",
                table: "Menu",
                column: "routerLink",
                unique: true,
                filter: "[routerLink] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MenuApi_ApiGUID",
                table: "MenuApi",
                column: "ApiGUID");

            migrationBuilder.CreateIndex(
                name: "IX_MenuApi_MenuGUID",
                table: "MenuApi",
                column: "MenuGUID");

            migrationBuilder.CreateIndex(
                name: "IX_OrgLocation_DeletedBy",
                table: "OrgLocation",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_OrgLocation_Gkey",
                table: "OrgLocation",
                column: "Gkey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrgLocation_Level",
                table: "OrgLocation",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_OrgLocation_Path",
                table: "OrgLocation",
                column: "Path",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrgLocation_POrgLocationGUID",
                table: "OrgLocation",
                column: "POrgLocationGUID");

            migrationBuilder.CreateIndex(
                name: "IX_OrglocationPackage_DeletedBy",
                table: "OrglocationPackage",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_OrglocationPackage_OrgLocationGUID",
                table: "OrglocationPackage",
                column: "OrgLocationGUID");

            migrationBuilder.CreateIndex(
                name: "IX_OrglocationPackage_PackageGUID",
                table: "OrglocationPackage",
                column: "PackageGUID");

            migrationBuilder.CreateIndex(
                name: "IX_Package_DeletedBy",
                table: "Package",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Package_RoleGUID",
                table: "Package",
                column: "RoleGUID");

            migrationBuilder.CreateIndex(
                name: "IX_Role_DeletedBy",
                table: "Role",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Role_Gkey",
                table: "Role",
                column: "Gkey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Role_Level",
                table: "Role",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_Role_OrgLocationGUID",
                table: "Role",
                column: "OrgLocationGUID");

            migrationBuilder.CreateIndex(
                name: "IX_Role_Path",
                table: "Role",
                column: "Path",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Role_PRoleGUID",
                table: "Role",
                column: "PRoleGUID");

            migrationBuilder.CreateIndex(
                name: "IX_RoleAccess_ApiGUID",
                table: "RoleAccess",
                column: "ApiGUID");

            migrationBuilder.CreateIndex(
                name: "IX_RoleAccess_DeletedBy",
                table: "RoleAccess",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RoleAccess_Gkey",
                table: "RoleAccess",
                column: "Gkey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoleAccess_Level",
                table: "RoleAccess",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_RoleAccess_MenuGUID",
                table: "RoleAccess",
                column: "MenuGUID");

            migrationBuilder.CreateIndex(
                name: "IX_RoleAccess_OrglocationPackageGUID",
                table: "RoleAccess",
                column: "OrglocationPackageGUID");

            migrationBuilder.CreateIndex(
                name: "IX_RoleAccess_Path",
                table: "RoleAccess",
                column: "Path",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoleAccess_PRoleAccessGUID",
                table: "RoleAccess",
                column: "PRoleAccessGUID");

            migrationBuilder.CreateIndex(
                name: "IX_RoleAccess_RoleGUID",
                table: "RoleAccess",
                column: "RoleGUID");

            migrationBuilder.CreateIndex(
                name: "IX_sso_Key",
                table: "sso",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sso_PackageGUID",
                table: "sso",
                column: "PackageGUID");

            migrationBuilder.CreateIndex(
                name: "IX_sso_UserGUID",
                table: "sso",
                column: "UserGUID");

            migrationBuilder.CreateIndex(
                name: "IX_User_DeletedBy",
                table: "User",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_User_Email",
                table: "User",
                column: "Email",
                unique: true,
                filter: "[Email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_User_Mobile",
                table: "User",
                column: "Mobile",
                unique: true,
                filter: "[Mobile] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_User_Username",
                table: "User",
                column: "Username",
                unique: true,
                filter: "[Username] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserInvite_DeletedBy",
                table: "UserInvite",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_UserInvite_OrgLocationGUID",
                table: "UserInvite",
                column: "OrgLocationGUID");

            migrationBuilder.CreateIndex(
                name: "IX_UserInvite_UserGUID",
                table: "UserInvite",
                column: "UserGUID");

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_DeletedBy",
                table: "UserRole",
                column: "DeletedBy");

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_Gkey",
                table: "UserRole",
                column: "Gkey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_OrgLocationGUID",
                table: "UserRole",
                column: "OrgLocationGUID");

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_RoleGUID",
                table: "UserRole",
                column: "RoleGUID");

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_UserGUID",
                table: "UserRole",
                column: "UserGUID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MenuApi");

            migrationBuilder.DropTable(
                name: "RoleAccess");

            migrationBuilder.DropTable(
                name: "sso");

            migrationBuilder.DropTable(
                name: "UserInvite");

            migrationBuilder.DropTable(
                name: "UserRole");

            migrationBuilder.DropTable(
                name: "Api");

            migrationBuilder.DropTable(
                name: "Menu");

            migrationBuilder.DropTable(
                name: "OrglocationPackage");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "SubSystem");

            migrationBuilder.DropTable(
                name: "Package");

            migrationBuilder.DropTable(
                name: "Role");

            migrationBuilder.DropTable(
                name: "OrgLocation");
        }
    }
}
