using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CallXApi.Migrations
{
    /// <inheritdoc />
    public partial class wes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "network_types",
                table: "network_reports",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "phone_type",
                table: "network_reports",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "state",
                table: "network_reports",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "network_types",
                table: "network_reports");

            migrationBuilder.DropColumn(
                name: "phone_type",
                table: "network_reports");

            migrationBuilder.DropColumn(
                name: "state",
                table: "network_reports");
        }
    }
}
