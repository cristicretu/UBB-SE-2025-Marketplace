using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace server.Migrations
{
    /// <inheritdoc />
    public partial class mig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Baskets",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    buyer_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Baskets", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ProductCategories",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    title = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCategories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ProductConditions",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    title = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductConditions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ProductTags",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    title = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductTags", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    rating = table.Column<double>(type: "float", nullable: false),
                    seller_id = table.Column<int>(type: "int", nullable: false),
                    reviewer_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    username = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    passwordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    userType = table.Column<int>(type: "int", nullable: false),
                    balance = table.Column<double>(type: "float", nullable: false),
                    rating = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "AuctionProductProductTags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    TagId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuctionProductProductTags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuctionProductProductTags_ProductTags_TagId",
                        column: x => x.TagId,
                        principalTable: "ProductTags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReviewsPictures",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    review_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewsPictures", x => x.id);
                    table.ForeignKey(
                        name: "FK_ReviewsPictures_Reviews_review_id",
                        column: x => x.review_id,
                        principalTable: "Reviews",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuctionProducts",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    seller_id = table.Column<int>(type: "int", nullable: false),
                    condition_id = table.Column<int>(type: "int", nullable: true),
                    category_id = table.Column<int>(type: "int", nullable: true),
                    start_datetime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    end_datetime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    starting_price = table.Column<double>(type: "float", nullable: false),
                    current_price = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuctionProducts", x => x.id);
                    table.ForeignKey(
                        name: "FK_AuctionProducts_ProductCategories_category_id",
                        column: x => x.category_id,
                        principalTable: "ProductCategories",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_AuctionProducts_ProductConditions_condition_id",
                        column: x => x.condition_id,
                        principalTable: "ProductConditions",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_AuctionProducts_Users_seller_id",
                        column: x => x.seller_id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BorrowProducts",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    time_limit = table.Column<DateTime>(type: "datetime2", nullable: false),
                    start_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    end_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    daily_rate = table.Column<double>(type: "float", nullable: false),
                    is_borrowed = table.Column<bool>(type: "bit", nullable: false),
                    title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    seller_id = table.Column<int>(type: "int", nullable: false),
                    condition_id = table.Column<int>(type: "int", nullable: false),
                    category_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BorrowProducts", x => x.id);
                    table.ForeignKey(
                        name: "FK_BorrowProducts_ProductCategories_category_id",
                        column: x => x.category_id,
                        principalTable: "ProductCategories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BorrowProducts_ProductConditions_condition_id",
                        column: x => x.condition_id,
                        principalTable: "ProductConditions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BorrowProducts_Users_seller_id",
                        column: x => x.seller_id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BuyProducts",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    seller_id = table.Column<int>(type: "int", nullable: false),
                    condition_id = table.Column<int>(type: "int", nullable: true),
                    category_id = table.Column<int>(type: "int", nullable: true),
                    price = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuyProducts", x => x.id);
                    table.ForeignKey(
                        name: "FK_BuyProducts_ProductCategories_category_id",
                        column: x => x.category_id,
                        principalTable: "ProductCategories",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_BuyProducts_ProductConditions_condition_id",
                        column: x => x.condition_id,
                        principalTable: "ProductConditions",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_BuyProducts_Users_seller_id",
                        column: x => x.seller_id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Conversations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conversations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Conversations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Cost = table.Column<double>(type: "float", nullable: false),
                    SellerId = table.Column<int>(type: "int", nullable: false),
                    BuyerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_Users_SellerId",
                        column: x => x.SellerId,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AuctionProductsImages",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    product_id = table.Column<int>(type: "int", nullable: false),
                    url = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuctionProductsImages", x => x.id);
                    table.ForeignKey(
                        name: "FK_AuctionProductsImages_AuctionProducts_product_id",
                        column: x => x.product_id,
                        principalTable: "AuctionProducts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Bids",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    bidder_id = table.Column<int>(type: "int", nullable: false),
                    product_id = table.Column<int>(type: "int", nullable: false),
                    price = table.Column<double>(type: "float", nullable: false),
                    timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bids", x => x.id);
                    table.ForeignKey(
                        name: "FK_Bids_AuctionProducts_product_id",
                        column: x => x.product_id,
                        principalTable: "AuctionProducts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Bids_Users_bidder_id",
                        column: x => x.bidder_id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BorrowProductImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    product_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BorrowProductImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BorrowProductImages_BorrowProducts_product_id",
                        column: x => x.product_id,
                        principalTable: "BorrowProducts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BorrowProductProductTags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    product_id = table.Column<int>(type: "int", nullable: false),
                    tag_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BorrowProductProductTags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BorrowProductProductTags_BorrowProducts_product_id",
                        column: x => x.product_id,
                        principalTable: "BorrowProducts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BorrowProductProductTags_ProductTags_tag_id",
                        column: x => x.tag_id,
                        principalTable: "ProductTags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BasketItemsBuyProducts",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    basket_id = table.Column<int>(type: "int", nullable: false),
                    product_id = table.Column<int>(type: "int", nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: false),
                    price = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BasketItemsBuyProducts", x => x.id);
                    table.ForeignKey(
                        name: "FK_BasketItemsBuyProducts_BuyProducts_product_id",
                        column: x => x.product_id,
                        principalTable: "BuyProducts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BuyProductImages",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    product_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuyProductImages", x => x.id);
                    table.ForeignKey(
                        name: "FK_BuyProductImages_BuyProducts_product_id",
                        column: x => x.product_id,
                        principalTable: "BuyProducts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BuyProductProductTags",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    product_id = table.Column<int>(type: "int", nullable: false),
                    tag_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuyProductProductTags", x => x.id);
                    table.ForeignKey(
                        name: "FK_BuyProductProductTags_BuyProducts_product_id",
                        column: x => x.product_id,
                        principalTable: "BuyProducts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BuyProductProductTags_ProductTags_tag_id",
                        column: x => x.tag_id,
                        principalTable: "ProductTags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConversationId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Messages_Conversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "Conversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Messages_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuctionProductProductTags_TagId",
                table: "AuctionProductProductTags",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_AuctionProducts_category_id",
                table: "AuctionProducts",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_AuctionProducts_condition_id",
                table: "AuctionProducts",
                column: "condition_id");

            migrationBuilder.CreateIndex(
                name: "IX_AuctionProducts_seller_id",
                table: "AuctionProducts",
                column: "seller_id");

            migrationBuilder.CreateIndex(
                name: "IX_AuctionProductsImages_product_id",
                table: "AuctionProductsImages",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_BasketItemsBuyProducts_product_id",
                table: "BasketItemsBuyProducts",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_Bids_bidder_id",
                table: "Bids",
                column: "bidder_id");

            migrationBuilder.CreateIndex(
                name: "IX_Bids_product_id",
                table: "Bids",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_BorrowProductImages_product_id",
                table: "BorrowProductImages",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_BorrowProductProductTags_product_id",
                table: "BorrowProductProductTags",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_BorrowProductProductTags_tag_id",
                table: "BorrowProductProductTags",
                column: "tag_id");

            migrationBuilder.CreateIndex(
                name: "IX_BorrowProducts_category_id",
                table: "BorrowProducts",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_BorrowProducts_condition_id",
                table: "BorrowProducts",
                column: "condition_id");

            migrationBuilder.CreateIndex(
                name: "IX_BorrowProducts_seller_id",
                table: "BorrowProducts",
                column: "seller_id");

            migrationBuilder.CreateIndex(
                name: "IX_BuyProductImages_product_id",
                table: "BuyProductImages",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_BuyProductProductTags_product_id",
                table: "BuyProductProductTags",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_BuyProductProductTags_tag_id",
                table: "BuyProductProductTags",
                column: "tag_id");

            migrationBuilder.CreateIndex(
                name: "IX_BuyProducts_category_id",
                table: "BuyProducts",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_BuyProducts_condition_id",
                table: "BuyProducts",
                column: "condition_id");

            migrationBuilder.CreateIndex(
                name: "IX_BuyProducts_seller_id",
                table: "BuyProducts",
                column: "seller_id");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_UserId",
                table: "Conversations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ConversationId",
                table: "Messages",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_UserId",
                table: "Messages",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_SellerId",
                table: "Orders",
                column: "SellerId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategories_title",
                table: "ProductCategories",
                column: "title",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductConditions_title",
                table: "ProductConditions",
                column: "title",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductTags_title",
                table: "ProductTags",
                column: "title",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReviewsPictures_review_id",
                table: "ReviewsPictures",
                column: "review_id");

            migrationBuilder.CreateIndex(
                name: "IX_Users_email",
                table: "Users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_username",
                table: "Users",
                column: "username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuctionProductProductTags");

            migrationBuilder.DropTable(
                name: "AuctionProductsImages");

            migrationBuilder.DropTable(
                name: "BasketItemsBuyProducts");

            migrationBuilder.DropTable(
                name: "Baskets");

            migrationBuilder.DropTable(
                name: "Bids");

            migrationBuilder.DropTable(
                name: "BorrowProductImages");

            migrationBuilder.DropTable(
                name: "BorrowProductProductTags");

            migrationBuilder.DropTable(
                name: "BuyProductImages");

            migrationBuilder.DropTable(
                name: "BuyProductProductTags");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "ReviewsPictures");

            migrationBuilder.DropTable(
                name: "AuctionProducts");

            migrationBuilder.DropTable(
                name: "BorrowProducts");

            migrationBuilder.DropTable(
                name: "BuyProducts");

            migrationBuilder.DropTable(
                name: "ProductTags");

            migrationBuilder.DropTable(
                name: "Conversations");

            migrationBuilder.DropTable(
                name: "Reviews");

            migrationBuilder.DropTable(
                name: "ProductCategories");

            migrationBuilder.DropTable(
                name: "ProductConditions");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
