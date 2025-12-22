using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shared.Users.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateUsersSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsModule = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ModuleName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsModule = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ModuleName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LastLoginAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Role_Permission",
                columns: table => new
                {
                    PermissionsName = table.Column<string>(type: "character varying(256)", nullable: false),
                    RolesName = table.Column<string>(type: "character varying(256)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role_Permission", x => new { x.PermissionsName, x.RolesName });
                    table.ForeignKey(
                        name: "FK_Role_Permission_Permissions_PermissionsName",
                        column: x => x.PermissionsName,
                        principalTable: "Permissions",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Role_Permission_Roles_RolesName",
                        column: x => x.RolesName,
                        principalTable: "Roles",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExternalProviders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Provider = table.Column<int>(type: "integer", nullable: false),
                    ExternalUserId = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    AddedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalProviders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExternalProviders_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "User_Role",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    RolesName = table.Column<string>(type: "character varying(256)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User_Role", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_User_Role_Roles_RolesName",
                        column: x => x.RolesName,
                        principalTable: "Roles",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_User_Role_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExternalProviders_Provider_ExternalUserId",
                table: "ExternalProviders",
                columns: new[] { "Provider", "ExternalUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExternalProviders_UserId",
                table: "ExternalProviders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_IsActive",
                table: "Permissions",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_IsModule",
                table: "Permissions",
                column: "IsModule");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_ModuleName",
                table: "Permissions",
                column: "ModuleName");

            migrationBuilder.CreateIndex(
                name: "IX_Role_Permission_RolesName",
                table: "Role_Permission",
                column: "RolesName");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_IsActive",
                table: "Roles",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_IsModule",
                table: "Roles",
                column: "IsModule");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_ModuleName",
                table: "Roles",
                column: "ModuleName");

            migrationBuilder.CreateIndex(
                name: "IX_User_Role_RoleId",
                table: "User_Role",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_User_Role_RolesName",
                table: "User_Role",
                column: "RolesName");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExternalProviders");

            migrationBuilder.DropTable(
                name: "Role_Permission");

            migrationBuilder.DropTable(
                name: "User_Role");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
