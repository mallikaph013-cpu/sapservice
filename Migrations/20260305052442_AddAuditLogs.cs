using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace myapp.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentRoutings_Departments_DepartmentId",
                table: "DocumentRoutings");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentRoutings_DocumentTypes_DocumentTypeId",
                table: "DocumentRoutings");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentRoutings_Plants_PlantId",
                table: "DocumentRoutings");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentRoutings_Sections_SectionId",
                table: "DocumentRoutings");

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EntityName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    EntityId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Action = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    PerformedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    PerformedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Details = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_PerformedAt",
                table: "AuditLogs",
                column: "PerformedAt");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentRoutings_Departments_DepartmentId",
                table: "DocumentRoutings",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "DepartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentRoutings_DocumentTypes_DocumentTypeId",
                table: "DocumentRoutings",
                column: "DocumentTypeId",
                principalTable: "DocumentTypes",
                principalColumn: "DocumentTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentRoutings_Plants_PlantId",
                table: "DocumentRoutings",
                column: "PlantId",
                principalTable: "Plants",
                principalColumn: "PlantId");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentRoutings_Sections_SectionId",
                table: "DocumentRoutings",
                column: "SectionId",
                principalTable: "Sections",
                principalColumn: "SectionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentRoutings_Departments_DepartmentId",
                table: "DocumentRoutings");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentRoutings_DocumentTypes_DocumentTypeId",
                table: "DocumentRoutings");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentRoutings_Plants_PlantId",
                table: "DocumentRoutings");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentRoutings_Sections_SectionId",
                table: "DocumentRoutings");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentRoutings_Departments_DepartmentId",
                table: "DocumentRoutings",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "DepartmentId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentRoutings_DocumentTypes_DocumentTypeId",
                table: "DocumentRoutings",
                column: "DocumentTypeId",
                principalTable: "DocumentTypes",
                principalColumn: "DocumentTypeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentRoutings_Plants_PlantId",
                table: "DocumentRoutings",
                column: "PlantId",
                principalTable: "Plants",
                principalColumn: "PlantId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentRoutings_Sections_SectionId",
                table: "DocumentRoutings",
                column: "SectionId",
                principalTable: "Sections",
                principalColumn: "SectionId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
