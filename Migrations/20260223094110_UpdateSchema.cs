using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace myapp.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Sections",
                newName: "SectionName");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Sections",
                newName: "SectionId");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Plants",
                newName: "PlantName");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Plants",
                newName: "PlantId");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Departments",
                newName: "DepartmentName");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Departments",
                newName: "DepartmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SectionName",
                table: "Sections",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "SectionId",
                table: "Sections",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "PlantName",
                table: "Plants",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "PlantId",
                table: "Plants",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "DepartmentName",
                table: "Departments",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "DepartmentId",
                table: "Departments",
                newName: "Id");
        }
    }
}
