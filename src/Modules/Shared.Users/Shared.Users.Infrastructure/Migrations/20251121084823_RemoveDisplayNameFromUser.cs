using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shared.Users.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDisplayNameFromUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "Users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "Users",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
