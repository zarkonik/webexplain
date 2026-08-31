using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebExplain.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddGuideStepTargetBounds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "TargetHeight",
                table: "GuideSteps",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "TargetWidth",
                table: "GuideSteps",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "TargetX",
                table: "GuideSteps",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "TargetY",
                table: "GuideSteps",
                type: "double precision",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TargetHeight",
                table: "GuideSteps");

            migrationBuilder.DropColumn(
                name: "TargetWidth",
                table: "GuideSteps");

            migrationBuilder.DropColumn(
                name: "TargetX",
                table: "GuideSteps");

            migrationBuilder.DropColumn(
                name: "TargetY",
                table: "GuideSteps");
        }
    }
}
