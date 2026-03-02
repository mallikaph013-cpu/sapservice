using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace myapp.Migrations
{
    /// <inheritdoc />
    public partial class AddEditBomFgFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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
                newName: "EditBomFg");

            migrationBuilder.AddColumn<bool>(
                name: "EditBomAllFg",
                table: "RequestItems",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "BomEditComponent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ItemCodeFrom = table.Column<string>(type: "TEXT", nullable: true),
                    DescriptionFrom = table.Column<string>(type: "TEXT", nullable: true),
                    ItemQuantityFrom = table.Column<decimal>(type: "TEXT", nullable: true),
                    UnitFrom = table.Column<string>(type: "TEXT", nullable: true),
                    BomUsageFrom = table.Column<string>(type: "TEXT", nullable: true),
                    SlocFrom = table.Column<string>(type: "TEXT", nullable: true),
                    ItemCodeTo = table.Column<string>(type: "TEXT", nullable: true),
                    DescriptionTo = table.Column<string>(type: "TEXT", nullable: true),
                    ItemQuantityTo = table.Column<decimal>(type: "TEXT", nullable: true),
                    UnitTo = table.Column<string>(type: "TEXT", nullable: true),
                    BomUsageTo = table.Column<string>(type: "TEXT", nullable: true),
                    SlocTo = table.Column<string>(type: "TEXT", nullable: true),
                    PlantTo = table.Column<string>(type: "TEXT", nullable: true),
                    RequestItemId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BomEditComponent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BomEditComponent_RequestItems_RequestItemId",
                        column: x => x.RequestItemId,
                        principalTable: "RequestItems",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BomEditComponent_RequestItemId",
                table: "BomEditComponent",
                column: "RequestItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BomEditComponent");

            migrationBuilder.DropColumn(
                name: "EditBomAllFg",
                table: "RequestItems");

            migrationBuilder.RenameColumn(
                name: "EditBomFg",
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
    }
}
