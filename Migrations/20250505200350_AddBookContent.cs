using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SesliKitapWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddBookContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "Books",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Content",
                table: "Books");
        }
    }
}
