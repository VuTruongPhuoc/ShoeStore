using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShoeStore.Migrations
{
    /// <inheritdoc />
    public partial class updatenews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Alias",
                table: "News");

            migrationBuilder.AddColumn<string>(
                name: "Postedby",
                table: "News",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Postedby",
                table: "News");

            migrationBuilder.AddColumn<string>(
                name: "Alias",
                table: "News",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
