using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShoeStore.Migrations
{
    /// <inheritdoc />
    public partial class updateAddReviews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WishLists_Product",
                table: "WishLists");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "WishLists",
                newName: "ProductDetailId");

            migrationBuilder.RenameIndex(
                name: "IX_WishLists_ProductId",
                table: "WishLists",
                newName: "IX_WishLists_ProductDetailId");

            migrationBuilder.AddForeignKey(
                name: "FK_WishLists_ProductDetail_ProductDetailId",
                table: "WishLists",
                column: "ProductDetailId",
                principalTable: "ProductDetail",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WishLists_ProductDetail_ProductDetailId",
                table: "WishLists");

            migrationBuilder.RenameColumn(
                name: "ProductDetailId",
                table: "WishLists",
                newName: "ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_WishLists_ProductDetailId",
                table: "WishLists",
                newName: "IX_WishLists_ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_WishLists_Product",
                table: "WishLists",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id");
        }
    }
}
