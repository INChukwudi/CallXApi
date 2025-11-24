using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CallXApi.Migrations
{
    /// <inheritdoc />
    public partial class @operator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "admin_users",
                type: "text",
                nullable: true,
                oldClrType: typeof(short),
                oldType: "smallint",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "created_by",
                table: "admin_users",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "department",
                table: "admin_users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "provider",
                table: "admin_users",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "created_by",
                table: "admin_users");

            migrationBuilder.DropColumn(
                name: "department",
                table: "admin_users");

            migrationBuilder.DropColumn(
                name: "provider",
                table: "admin_users");

            migrationBuilder.AlterColumn<short>(
                name: "status",
                table: "admin_users",
                type: "smallint",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
