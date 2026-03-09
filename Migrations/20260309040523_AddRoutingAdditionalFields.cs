using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace myapp.Migrations
{
    /// <inheritdoc />
    public partial class AddRoutingAdditionalFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Alternative",
                table: "Routings",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BomUsage",
                table: "Routings",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Counter",
                table: "Routings",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Plant",
                table: "Routings",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Alternative",
                table: "Routings");

            migrationBuilder.DropColumn(
                name: "BomUsage",
                table: "Routings");

            migrationBuilder.DropColumn(
                name: "Counter",
                table: "Routings");

            migrationBuilder.DropColumn(
                name: "Plant",
                table: "Routings");
        }
    }
}
