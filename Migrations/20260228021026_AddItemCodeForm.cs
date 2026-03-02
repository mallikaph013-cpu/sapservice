using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace myapp.Migrations
{
    /// <inheritdoc />
    public partial class AddItemCodeForm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ItemCodeForm",
                table: "RequestItems",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ItemCodeForm",
                table: "RequestItems");
        }
    }
}
