using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebExplain.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddGuideStepElementDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ElementDescription",
                table: "GuideSteps",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ElementDescription",
                table: "GuideSteps");
        }
    }
}
