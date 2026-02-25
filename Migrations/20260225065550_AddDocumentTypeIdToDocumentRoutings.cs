using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace myapp.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentTypeIdToDocumentRoutings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "DocumentTypes");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "DocumentTypes",
                newName: "DocumentTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DocumentTypeId",
                table: "DocumentTypes",
                newName: "Id");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "DocumentTypes",
                type: "TEXT",
                maxLength: 255,
                nullable: true);
        }
    }
}
