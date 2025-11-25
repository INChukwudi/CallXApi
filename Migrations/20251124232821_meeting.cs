using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CallXApi.Migrations
{
    /// <inheritdoc />
    public partial class meeting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "issue_type",
                table: "network_reports",
                newName: "experience_type");

            migrationBuilder.AddColumn<string>(
                name: "report_category",
                table: "network_reports",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "report_category",
                table: "network_reports");

            migrationBuilder.RenameColumn(
                name: "experience_type",
                table: "network_reports",
                newName: "issue_type");
        }
    }
}
