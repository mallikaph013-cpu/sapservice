using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace myapp.Migrations
{
    /// <inheritdoc />
    public partial class AddDescriptionFrom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ItemCodeForm",
                table: "RequestItems",
                newName: "UnitTo");

            migrationBuilder.AddColumn<string>(
                name: "BomUsageFrom",
                table: "RequestItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BomUsageTo",
                table: "RequestItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionFrom",
                table: "RequestItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionTo",
                table: "RequestItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ItemCodeTo",
                table: "RequestItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ItemQuantityFrom",
                table: "RequestItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ItemQuantityTo",
                table: "RequestItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlantTo",
                table: "RequestItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SlocFrom",
                table: "RequestItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SlocTo",
                table: "RequestItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UnitFrom",
                table: "RequestItems",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BomUsageFrom",
                table: "RequestItems");

            migrationBuilder.DropColumn(
                name: "BomUsageTo",
                table: "RequestItems");

            migrationBuilder.DropColumn(
                name: "DescriptionFrom",
                table: "RequestItems");

            migrationBuilder.DropColumn(
                name: "DescriptionTo",
                table: "RequestItems");

            migrationBuilder.DropColumn(
                name: "ItemCodeTo",
                table: "RequestItems");

            migrationBuilder.DropColumn(
                name: "ItemQuantityFrom",
                table: "RequestItems");

            migrationBuilder.DropColumn(
                name: "ItemQuantityTo",
                table: "RequestItems");

            migrationBuilder.DropColumn(
                name: "PlantTo",
                table: "RequestItems");

            migrationBuilder.DropColumn(
                name: "SlocFrom",
                table: "RequestItems");

            migrationBuilder.DropColumn(
                name: "SlocTo",
                table: "RequestItems");

            migrationBuilder.DropColumn(
                name: "UnitFrom",
                table: "RequestItems");

            migrationBuilder.RenameColumn(
                name: "UnitTo",
                table: "RequestItems",
                newName: "ItemCodeForm");
        }
    }
}
