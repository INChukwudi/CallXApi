using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CallXApi.Migrations
{
    /// <inheritdoc />
    public partial class reportdesc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "network_reports",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "description",
                table: "network_reports");
        }
    }
}
