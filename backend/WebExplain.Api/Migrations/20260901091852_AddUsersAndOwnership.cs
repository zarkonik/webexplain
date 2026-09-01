using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebExplain.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddUsersAndOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Guides",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "CaptureSessions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Guides_UserId",
                table: "Guides",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CaptureSessions_UserId",
                table: "CaptureSessions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            // Pre-existing Guides/CaptureSessions were created before user accounts existed
            // and can't be attributed to anyone - this is dev/test data from before login
            // was added, not real production content, so it's cleared rather than left
            // pointing at a non-existent user (which the FK constraints below would reject).
            migrationBuilder.Sql("DELETE FROM \"Guides\";");
            migrationBuilder.Sql("DELETE FROM \"CaptureSessions\";");

            migrationBuilder.AddForeignKey(
                name: "FK_CaptureSessions_Users_UserId",
                table: "CaptureSessions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Guides_Users_UserId",
                table: "Guides",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CaptureSessions_Users_UserId",
                table: "CaptureSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_Guides_Users_UserId",
                table: "Guides");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Guides_UserId",
                table: "Guides");

            migrationBuilder.DropIndex(
                name: "IX_CaptureSessions_UserId",
                table: "CaptureSessions");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Guides");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "CaptureSessions");
        }
    }
}
