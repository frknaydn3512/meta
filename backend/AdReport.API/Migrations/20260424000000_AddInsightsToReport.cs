using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdReport.API.Migrations
{
    /// <inheritdoc />
    public partial class AddInsightsToReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InsightsJson",
                table: "Reports",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CampaignsJson",
                table: "Reports",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "InsightsJson", table: "Reports");
            migrationBuilder.DropColumn(name: "CampaignsJson", table: "Reports");
        }
    }
}
