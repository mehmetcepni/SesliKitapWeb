using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SesliKitapWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddIsFavoriteToUserBook : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFavorite",
                table: "UserBooks",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFavorite",
                table: "UserBooks");
        }
    }
}
