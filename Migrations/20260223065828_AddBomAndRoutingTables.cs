using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace myapp.Migrations
{
    /// <inheritdoc />
    public partial class AddBomAndRoutingTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "RequestItems",
                newName: "Transportation");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "RequestItems",
                newName: "ToolingBSection");

            migrationBuilder.AddColumn<string>(
                name: "AccountAssignment",
                table: "RequestItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AsiOfPlant",
                table: "RequestItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AssemblyPlant",
                table: "RequestItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Availability",
                table: "RequestItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BoiDescription",
                table: "RequestItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Check",
                table: "RequestItems",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CodenMid",
                table: "RequestItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CommCodeTariffCode",
                table: "RequestItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "RequestItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CurrentICS",
                table: "RequestItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateIn",
                table: "RequestItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DevicePlant",
                table: "RequestItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Effective",
                table: "RequestItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalMaterialGroup",
                table: "RequestItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FixedLot",
                table: "RequestItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GeneralItemCategory",
                table: "RequestItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IpoPlant",
                table: "RequestItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Level",
                table: "RequestItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LoadingGroup",
                table: "RequestItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MakerMfrPartNumber",
                table: "RequestItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MatType",
                table: "RequestItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MaterialStatisticsGroup",
                table: "RequestItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MaxLot",
                table: "RequestItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MinLot",
                table: "RequestItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Mtlsm",
                table: "RequestItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlanDelTime",
                table: "RequestItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Planner",
                table: "RequestItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PoNumber",
                table: "RequestItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PriceControl",
                table: "RequestItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PriceUnit",
                table: "RequestItems",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PurchasingGroup",
                table: "RequestItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QuotationNumber",
                table: "RequestItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReceiveStorage",
                table: "RequestItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestType",
                table: "RequestItems",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Rohs",
                table: "RequestItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Rounding",
                table: "RequestItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SalesOrg",
                table: "RequestItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SchedMargin",
                table: "RequestItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatusInA",
                table: "RequestItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StorageLoc",
                table: "RequestItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StorageLocationB1",
                table: "RequestItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StorageLocationEP",
                table: "RequestItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupplierCode",
                table: "RequestItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TariffCode",
                table: "RequestItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaxTh",
                table: "RequestItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ToolingBModel",
                table: "RequestItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TraffCodePercentage",
                table: "RequestItems",
                type: "decimal(18, 2)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BomComponents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RequestItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    Level = table.Column<int>(type: "INTEGER", nullable: false),
                    Item = table.Column<string>(type: "TEXT", nullable: true),
                    ItemCat = table.Column<string>(type: "TEXT", nullable: true),
                    ComponentNumber = table.Column<string>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    ItemQuantity = table.Column<decimal>(type: "decimal(18, 5)", nullable: true),
                    Unit = table.Column<string>(type: "TEXT", nullable: true),
                    BomUsage = table.Column<string>(type: "TEXT", nullable: true),
                    Plant = table.Column<string>(type: "TEXT", nullable: true),
                    Sloc = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BomComponents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BomComponents_RequestItems_RequestItemId",
                        column: x => x.RequestItemId,
                        principalTable: "RequestItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Routings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RequestItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    Material = table.Column<string>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    WorkCenter = table.Column<string>(type: "TEXT", nullable: true),
                    Operation = table.Column<string>(type: "TEXT", nullable: true),
                    BaseQty = table.Column<decimal>(type: "decimal(18, 5)", nullable: true),
                    Unit = table.Column<string>(type: "TEXT", nullable: true),
                    DirectLaborCosts = table.Column<decimal>(type: "decimal(18, 5)", nullable: true),
                    DirectExpenses = table.Column<decimal>(type: "decimal(18, 5)", nullable: true),
                    AllocationExpense = table.Column<decimal>(type: "decimal(18, 5)", nullable: true),
                    ProductionVersionCode = table.Column<string>(type: "TEXT", nullable: true),
                    Version = table.Column<string>(type: "TEXT", nullable: true),
                    ValidFrom = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ValidTo = table.Column<DateTime>(type: "TEXT", nullable: true),
                    MaximumLotSize = table.Column<decimal>(type: "decimal(18, 5)", nullable: true),
                    Group = table.Column<string>(type: "TEXT", nullable: true),
                    GroupCounter = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Routings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Routings_RequestItems_RequestItemId",
                        column: x => x.RequestItemId,
                        principalTable: "RequestItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BomComponents_RequestItemId",
                table: "BomComponents",
                column: "RequestItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Routings_RequestItemId",
                table: "Routings",
                column: "RequestItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BomComponents");

            migrationBuilder.DropTable(
                name: "Routings");

            migrationBuilder.DropColumn(
                name: "AccountAssignment",
                table: "RequestItems");

            migrationBuilder.DropColumn(
                name: "AsiOfPlant",
                table: "RequestItems");

            migrationBuilder.DropColumn(
                name: "AssemblyPlant",
                table: "RequestItems");

            migrationBuilder.DropColumn(
                name: "Availability",
                table: "RequestItems");

            migrationBuilder.DropColumn(
                name: "BoiDescription",
                table: "RequestItems");

            migrationBuilder.DropColumn(
                name: "Check",
                table: "RequestItems");

            migrationBuilder.DropColumn(
                name: "CodenMid",
                table: "RequestItems");

            migrationBuilder.DropColumn(
                name: "CommCodeTariffCode",
                table: "RequestItems");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "RequestItems");

            migrationBuilder.DropColumn(
                name: "CurrentICS",
                table: "RequestItems");

            migrationBuilder.DropColumn(
                name: "DateIn",
                table: "RequestItems");

            migrationBuilder.DropColumn(
                name: "DevicePlant",
                table: "RequestItems");

            migrationBuilder.DropColumn(
                name: "Effective",
                table: "RequestItems");

            migrationBuilder.DropColumn(
                name: "ExternalMaterialGroup",
                table: "RequestItems");

            migrationBuilder.DropColumn(
                name: "FixedLot",
                table: "RequestItems");

            migrationBuilder.DropColumn(
                name: "GeneralItemCategory",
                table: "RequestItems");

            migrationBuilder.DropColumn(
                name: "IpoPlant",
                table: "RequestItems");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "RequestItems");

            migrationBuilder.DropColumn(
                name: "LoadingGroup",
                table: "RequestItems");

            migrationBuilder.DropColumn(
                name: "MakerMfrPartNumber",
                table: "RequestItems");

            migrationBuilder.DropColumn(
                name: "MatType",
                table: "RequestItems");

            migrationBuilder.DropColumn(
                name: "MaterialStatisticsGroup",
                table: "RequestItems");

            migrationBuilder.DropColumn(
                name: "MaxLot",
                table: "RequestItems");

            migrationBuilder.DropColumn(
                name: "MinLot",
                table: "RequestItems");

            migrationBuilder.DropColumn(
                name: "Mtlsm",
                table: "RequestItems");

            migrationBuilder.DropColumn(
                name: "PlanDelTime",
                table: "RequestItems");

            migrationBuilder.DropColumn(
                name: "Planner",
                table: "RequestItems");

            migrationBuilder.DropColumn(
                name: "PoNumber",
                table: "RequestItems");

            migrationBuilder.DropColumn(
                name: "PriceControl",
                table: "RequestItems");

            migrationBuilder.DropColumn(
                name: "PriceUnit",
                table: "RequestItems");

            migrationBuilder.DropColumn(
                name: "PurchasingGroup",
                table: "RequestItems");

            migrationBuilder.DropColumn(
                name: "QuotationNumber",
                table: "RequestItems");

            migrationBuilder.DropColumn(
                name: "ReceiveStorage",
                table: "RequestItems");

            migrationBuilder.DropColumn(
                name: "RequestType",
                table: "RequestItems");

            migrationBuilder.DropColumn(
                name: "Rohs",
                table: "RequestItems");

            migrationBuilder.DropColumn(
                name: "Rounding",
                table: "RequestItems");

            migrationBuilder.DropColumn(
                name: "SalesOrg",
                table: "RequestItems");

            migrationBuilder.DropColumn(
                name: "SchedMargin",
                table: "RequestItems");

            migrationBuilder.DropColumn(
                name: "StatusInA",
                table: "RequestItems");

            migrationBuilder.DropColumn(
                name: "StorageLoc",
                table: "RequestItems");

            migrationBuilder.DropColumn(
                name: "StorageLocationB1",
                table: "RequestItems");

            migrationBuilder.DropColumn(
                name: "StorageLocationEP",
                table: "RequestItems");

            migrationBuilder.DropColumn(
                name: "SupplierCode",
                table: "RequestItems");

            migrationBuilder.DropColumn(
                name: "TariffCode",
                table: "RequestItems");

            migrationBuilder.DropColumn(
                name: "TaxTh",
                table: "RequestItems");

            migrationBuilder.DropColumn(
                name: "ToolingBModel",
                table: "RequestItems");

            migrationBuilder.DropColumn(
                name: "TraffCodePercentage",
                table: "RequestItems");

            migrationBuilder.RenameColumn(
                name: "Transportation",
                table: "RequestItems",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "ToolingBSection",
                table: "RequestItems",
                newName: "CreatedAt");
        }
    }
}
