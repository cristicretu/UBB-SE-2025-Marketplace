using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace server.Migrations
{
    /// <inheritdoc />
    public partial class DuplicateProductIdFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BuyerCartItems_BuyProducts_BuyProductId",
                table: "BuyerCartItems");

            migrationBuilder.DropForeignKey(
                name: "FK_BuyerWishlistItems_BuyProducts_BuyProductId",
                table: "BuyerWishlistItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_BuyProducts_BuyProductId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_BuyProductId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_BuyerWishlistItems_BuyProductId",
                table: "BuyerWishlistItems");

            migrationBuilder.DropIndex(
                name: "IX_BuyerCartItems_BuyProductId",
                table: "BuyerCartItems");

            migrationBuilder.DropColumn(
                name: "BuyProductId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "BuyProductId",
                table: "BuyerWishlistItems");

            migrationBuilder.DropColumn(
                name: "BuyProductId",
                table: "BuyerCartItems");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ProductID",
                table: "Orders",
                column: "ProductID");

            migrationBuilder.CreateIndex(
                name: "IX_BuyerWishlistItems_ProductId",
                table: "BuyerWishlistItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_BuyerCartItems_ProductId",
                table: "BuyerCartItems",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_BuyerCartItems_BuyProducts_ProductId",
                table: "BuyerCartItems",
                column: "ProductId",
                principalTable: "BuyProducts",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BuyerWishlistItems_BuyProducts_ProductId",
                table: "BuyerWishlistItems",
                column: "ProductId",
                principalTable: "BuyProducts",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_BuyProducts_ProductID",
                table: "Orders",
                column: "ProductID",
                principalTable: "BuyProducts",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BuyerCartItems_BuyProducts_ProductId",
                table: "BuyerCartItems");

            migrationBuilder.DropForeignKey(
                name: "FK_BuyerWishlistItems_BuyProducts_ProductId",
                table: "BuyerWishlistItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_BuyProducts_ProductID",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_ProductID",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_BuyerWishlistItems_ProductId",
                table: "BuyerWishlistItems");

            migrationBuilder.DropIndex(
                name: "IX_BuyerCartItems_ProductId",
                table: "BuyerCartItems");

            migrationBuilder.AddColumn<int>(
                name: "BuyProductId",
                table: "Orders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BuyProductId",
                table: "BuyerWishlistItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BuyProductId",
                table: "BuyerCartItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_BuyProductId",
                table: "Orders",
                column: "BuyProductId");

            migrationBuilder.CreateIndex(
                name: "IX_BuyerWishlistItems_BuyProductId",
                table: "BuyerWishlistItems",
                column: "BuyProductId");

            migrationBuilder.CreateIndex(
                name: "IX_BuyerCartItems_BuyProductId",
                table: "BuyerCartItems",
                column: "BuyProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_BuyerCartItems_BuyProducts_BuyProductId",
                table: "BuyerCartItems",
                column: "BuyProductId",
                principalTable: "BuyProducts",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BuyerWishlistItems_BuyProducts_BuyProductId",
                table: "BuyerWishlistItems",
                column: "BuyProductId",
                principalTable: "BuyProducts",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_BuyProducts_BuyProductId",
                table: "Orders",
                column: "BuyProductId",
                principalTable: "BuyProducts",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
