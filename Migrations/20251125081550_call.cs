using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CallXApi.Migrations
{
    /// <inheritdoc />
    public partial class call : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "call_direction",
                table: "network_reports",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "call_direction",
                table: "network_reports");
        }
    }
}
