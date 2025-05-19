using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace server.Migrations
{
    /// <inheritdoc />
    public partial class SchemaUpdateForBorrowBidsAuctionBuyProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuctionProducts_Users_seller_id",
                table: "AuctionProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_Bids_Users_bidder_id",
                table: "Bids");

            migrationBuilder.DropForeignKey(
                name: "FK_BorrowProducts_Users_seller_id",
                table: "BorrowProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_BuyProducts_Users_seller_id",
                table: "BuyProducts");

            migrationBuilder.AddForeignKey(
                name: "FK_AuctionProducts_Sellers_seller_id",
                table: "AuctionProducts",
                column: "seller_id",
                principalTable: "Sellers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Bids_Buyers_bidder_id",
                table: "Bids",
                column: "bidder_id",
                principalTable: "Buyers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BorrowProducts_Sellers_seller_id",
                table: "BorrowProducts",
                column: "seller_id",
                principalTable: "Sellers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BuyProducts_Sellers_seller_id",
                table: "BuyProducts",
                column: "seller_id",
                principalTable: "Sellers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuctionProducts_Sellers_seller_id",
                table: "AuctionProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_Bids_Buyers_bidder_id",
                table: "Bids");

            migrationBuilder.DropForeignKey(
                name: "FK_BorrowProducts_Sellers_seller_id",
                table: "BorrowProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_BuyProducts_Sellers_seller_id",
                table: "BuyProducts");

            migrationBuilder.AddForeignKey(
                name: "FK_AuctionProducts_Users_seller_id",
                table: "AuctionProducts",
                column: "seller_id",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Bids_Users_bidder_id",
                table: "Bids",
                column: "bidder_id",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BorrowProducts_Users_seller_id",
                table: "BorrowProducts",
                column: "seller_id",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BuyProducts_Users_seller_id",
                table: "BuyProducts",
                column: "seller_id",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
