using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace server.Migrations
{
    /// <inheritdoc />
    public partial class SchemaFix2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Reviews_reviewer_id",
                table: "Reviews",
                column: "reviewer_id");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_seller_id",
                table: "Reviews",
                column: "seller_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Buyers_reviewer_id",
                table: "Reviews",
                column: "reviewer_id",
                principalTable: "Buyers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Sellers_seller_id",
                table: "Reviews",
                column: "seller_id",
                principalTable: "Sellers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Buyers_reviewer_id",
                table: "Reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Sellers_seller_id",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_reviewer_id",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_seller_id",
                table: "Reviews");
        }
    }
}
