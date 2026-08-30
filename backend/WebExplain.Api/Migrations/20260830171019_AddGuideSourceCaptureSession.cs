using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebExplain.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddGuideSourceCaptureSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SourceCaptureSessionId",
                table: "Guides",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SourceCaptureSessionId",
                table: "Guides");
        }
    }
}
