using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebExplain.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddGuideStepPageUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PageUrl",
                table: "GuideSteps",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PageUrl",
                table: "GuideSteps");
        }
    }
}
