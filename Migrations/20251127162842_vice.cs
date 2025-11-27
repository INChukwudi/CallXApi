using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CallXApi.Migrations
{
    /// <inheritdoc />
    public partial class vice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "phone_type",
                table: "network_reports",
                newName: "network_type");

            migrationBuilder.RenameColumn(
                name: "network_types",
                table: "network_reports",
                newName: "device_model");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "network_type",
                table: "network_reports",
                newName: "phone_type");

            migrationBuilder.RenameColumn(
                name: "device_model",
                table: "network_reports",
                newName: "network_types");
        }
    }
}
