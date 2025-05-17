using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace server.Migrations
{
    /// <inheritdoc />
    public partial class Patapim : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuctionProducts_ProductCategories_category_id",
                table: "AuctionProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_AuctionProducts_ProductConditions_condition_id",
                table: "AuctionProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_BuyProducts_ProductCategories_category_id",
                table: "BuyProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_BuyProducts_ProductConditions_condition_id",
                table: "BuyProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Users_SellerId",
                table: "Orders");

            migrationBuilder.DropTable(
                name: "BasketItemsBuyProducts");

            migrationBuilder.DropTable(
                name: "Baskets");

            migrationBuilder.DropColumn(
                name: "rating",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "BuyerId",
                table: "Orders",
                newName: "BuyerID");

            migrationBuilder.AddColumn<DateTime>(
                name: "bannedUntil",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "failedLogIns",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "isBanned",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "phoneNumber",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "score",
                table: "Reviews",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BuyProductId",
                table: "Orders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "OrderDate",
                table: "Orders",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<int>(
                name: "OrderHistoryID",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OrderSummaryID",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ProductID",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ProductType",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "BuyProducts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "condition_id",
                table: "BuyProducts",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "category_id",
                table: "BuyProducts",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "stock",
                table: "BuyProducts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "price",
                table: "BorrowProducts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "stock",
                table: "BorrowProducts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "AuctionProducts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "condition_id",
                table: "AuctionProducts",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "category_id",
                table: "AuctionProducts",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "price",
                table: "AuctionProducts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "stock",
                table: "AuctionProducts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Addresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StreetLine = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PostalCode = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Addresses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrderHistory",
                columns: table => new
                {
                    OrderID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderHistory", x => x.OrderID);
                });

            migrationBuilder.CreateTable(
                name: "OrderSummary",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Subtotal = table.Column<double>(type: "float", nullable: false),
                    WarrantyTax = table.Column<double>(type: "float", nullable: false),
                    DeliveryFee = table.Column<double>(type: "float", nullable: false),
                    FinalTotal = table.Column<double>(type: "float", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PostalCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdditionalInfo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContractDetails = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderSummary", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "PDFs",
                columns: table => new
                {
                    PdfID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    File = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PDFs", x => x.PdfID);
                });

            migrationBuilder.CreateTable(
                name: "PredefinedContracts",
                columns: table => new
                {
                    ContractID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContractContent = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PredefinedContracts", x => x.ContractID);
                });

            migrationBuilder.CreateTable(
                name: "Sellers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FollowersCount = table.Column<int>(type: "int", nullable: false),
                    StoreName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StoreDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StoreAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TrustScore = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sellers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sellers_Users_Id",
                        column: x => x.Id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrackedOrders",
                columns: table => new
                {
                    TrackedOrderID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderID = table.Column<int>(type: "int", nullable: false),
                    CurrentStatus = table.Column<int>(type: "int", nullable: false),
                    EstimatedDeliveryDate = table.Column<DateOnly>(type: "date", nullable: false),
                    DeliveryAddress = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackedOrders", x => x.TrackedOrderID);
                    table.CheckConstraint("TrackedOrderConstraint", "[CurrentStatus] IN ('PROCESSING', 'SHIPPED', 'IN_WAREHOUSE', 'IN_TRANSIT', 'OUT_FOR_DELIVERY', 'DELIVERED')");
                    table.ForeignKey(
                        name: "FK_TrackedOrders_Orders_OrderID",
                        column: x => x.OrderID,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Buyers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UseSameAddress = table.Column<bool>(type: "bit", nullable: false),
                    Badge = table.Column<int>(type: "int", nullable: false),
                    TotalSpending = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    NumberOfPurchases = table.Column<int>(type: "int", nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ShippingAddressId = table.Column<int>(type: "int", nullable: false),
                    BillingAddressId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Buyers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Buyers_Addresses_BillingAddressId",
                        column: x => x.BillingAddressId,
                        principalTable: "Addresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Buyers_Addresses_ShippingAddressId",
                        column: x => x.ShippingAddressId,
                        principalTable: "Addresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Buyers_Users_Id",
                        column: x => x.Id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Contracts",
                columns: table => new
                {
                    ContractID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderID = table.Column<int>(type: "int", nullable: false),
                    ContractStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContractContent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RenewalCount = table.Column<int>(type: "int", nullable: false),
                    PredefinedContractID = table.Column<int>(type: "int", nullable: true),
                    PDFID = table.Column<int>(type: "int", nullable: false),
                    AdditionalTerms = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RenewedFromContractID = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contracts", x => x.ContractID);
                    table.CheckConstraint("ContractStatusConstraint", "[ContractStatus] IN ('ACTIVE', 'RENEWED', 'EXPIRED')");
                    table.ForeignKey(
                        name: "FK_Contracts_Orders_OrderID",
                        column: x => x.OrderID,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Contracts_PDFs_PDFID",
                        column: x => x.PDFID,
                        principalTable: "PDFs",
                        principalColumn: "PdfID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Contracts_PredefinedContracts_PredefinedContractID",
                        column: x => x.PredefinedContractID,
                        principalTable: "PredefinedContracts",
                        principalColumn: "ContractID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SellerNotifications",
                columns: table => new
                {
                    NotificationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SellerID = table.Column<int>(type: "int", nullable: false),
                    NotificationMessage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NotificationFollowerCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SellerNotifications", x => x.NotificationID);
                    table.ForeignKey(
                        name: "FK_SellerNotifications_Sellers_SellerID",
                        column: x => x.SellerID,
                        principalTable: "Sellers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderCheckpoints",
                columns: table => new
                {
                    CheckpointID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TrackedOrderID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderCheckpoints", x => x.CheckpointID);
                    table.CheckConstraint("OrderChekpointConstraint", "[Status] IN ('PROCESSING', 'SHIPPED', 'IN_WAREHOUSE', 'IN_TRANSIT', 'OUT_FOR_DELIVERY', 'DELIVERED')");
                    table.ForeignKey(
                        name: "FK_OrderCheckpoints_TrackedOrders_TrackedOrderID",
                        column: x => x.TrackedOrderID,
                        principalTable: "TrackedOrders",
                        principalColumn: "TrackedOrderID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BuyerCartItems",
                columns: table => new
                {
                    BuyerId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    BuyProductId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuyerCartItems", x => new { x.BuyerId, x.ProductId });
                    table.ForeignKey(
                        name: "FK_BuyerCartItems_BuyProducts_BuyProductId",
                        column: x => x.BuyProductId,
                        principalTable: "BuyProducts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BuyerCartItems_Buyers_BuyerId",
                        column: x => x.BuyerId,
                        principalTable: "Buyers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BuyerLinkages",
                columns: table => new
                {
                    RequestingBuyerId = table.Column<int>(type: "int", nullable: false),
                    ReceivingBuyerId = table.Column<int>(type: "int", nullable: false),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuyerLinkages", x => new { x.RequestingBuyerId, x.ReceivingBuyerId });
                    table.CheckConstraint("CK_BuyerLinkage_DifferentBuyers", "[RequestingBuyerId] <> [ReceivingBuyerId]");
                    table.ForeignKey(
                        name: "FK_BuyerLinkages_Buyers_ReceivingBuyerId",
                        column: x => x.ReceivingBuyerId,
                        principalTable: "Buyers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BuyerLinkages_Buyers_RequestingBuyerId",
                        column: x => x.RequestingBuyerId,
                        principalTable: "Buyers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BuyerWishlistItems",
                columns: table => new
                {
                    BuyerId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    BuyProductId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuyerWishlistItems", x => new { x.BuyerId, x.ProductId });
                    table.ForeignKey(
                        name: "FK_BuyerWishlistItems_BuyProducts_BuyProductId",
                        column: x => x.BuyProductId,
                        principalTable: "BuyProducts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BuyerWishlistItems_Buyers_BuyerId",
                        column: x => x.BuyerId,
                        principalTable: "Buyers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DummyCards",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BuyerId = table.Column<int>(type: "int", nullable: false),
                    CardholderName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CardNumber = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CVC = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Balance = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DummyCards", x => x.ID);
                    table.ForeignKey(
                        name: "FK_DummyCards_Buyers_BuyerId",
                        column: x => x.BuyerId,
                        principalTable: "Buyers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DummyWallets",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BuyerId = table.Column<int>(type: "int", nullable: false),
                    Balance = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DummyWallets", x => x.ID);
                    table.ForeignKey(
                        name: "FK_DummyWallets_Buyers_BuyerId",
                        column: x => x.BuyerId,
                        principalTable: "Buyers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Followings",
                columns: table => new
                {
                    BuyerId = table.Column<int>(type: "int", nullable: false),
                    SellerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Followings", x => new { x.BuyerId, x.SellerId });
                    table.ForeignKey(
                        name: "FK_Followings_Buyers_BuyerId",
                        column: x => x.BuyerId,
                        principalTable: "Buyers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Followings_Sellers_SellerId",
                        column: x => x.SellerId,
                        principalTable: "Sellers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserWaitList",
                columns: table => new
                {
                    UserWaitListID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductWaitListID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    JoinedTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PositionInQueue = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserWaitList", x => x.UserWaitListID);
                    table.ForeignKey(
                        name: "FK_UserWaitList_BorrowProducts_ProductWaitListID",
                        column: x => x.ProductWaitListID,
                        principalTable: "BorrowProducts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserWaitList_Buyers_UserID",
                        column: x => x.UserID,
                        principalTable: "Buyers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderNotifications",
                columns: table => new
                {
                    NotificationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecipientID = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    ContractID = table.Column<long>(type: "bigint", nullable: true),
                    IsAccepted = table.Column<bool>(type: "bit", nullable: true),
                    ProductID = table.Column<int>(type: "int", nullable: true),
                    OrderID = table.Column<int>(type: "int", nullable: true),
                    ShippingState = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeliveryDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ExpirationDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderNotifications", x => x.NotificationID);
                    table.CheckConstraint("NotificationCategoryConstraint", "[Category] IN ('CONTRACT_EXPIRATION', 'OUTBIDDED', 'ORDER_SHIPPING_PROGRESS', 'PRODUCT_AVAILABLE', 'PAYMENT_CONFIRMATION', 'PRODUCT_REMOVED', 'CONTRACT_RENEWAL_REQ', 'CONTRACT_RENEWAL_ANS', 'CONTRACT_RENEWAL_WAITLIST')");
                    table.ForeignKey(
                        name: "FK_OrderNotifications_BuyProducts_ProductID",
                        column: x => x.ProductID,
                        principalTable: "BuyProducts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderNotifications_Buyers_RecipientID",
                        column: x => x.RecipientID,
                        principalTable: "Buyers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderNotifications_Contracts_ContractID",
                        column: x => x.ContractID,
                        principalTable: "Contracts",
                        principalColumn: "ContractID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderNotifications_Orders_OrderID",
                        column: x => x.OrderID,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_BuyerID",
                table: "Orders",
                column: "BuyerID");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_BuyProductId",
                table: "Orders",
                column: "BuyProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderHistoryID",
                table: "Orders",
                column: "OrderHistoryID");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderSummaryID",
                table: "Orders",
                column: "OrderSummaryID");

            migrationBuilder.AddCheckConstraint(
                name: "PaymentMethodConstraint",
                table: "Orders",
                sql: "[PaymentMethod] IN ('card', 'wallet', 'cash')");

            migrationBuilder.CreateIndex(
                name: "IX_BuyerCartItems_BuyProductId",
                table: "BuyerCartItems",
                column: "BuyProductId");

            migrationBuilder.CreateIndex(
                name: "IX_BuyerLinkages_ReceivingBuyerId",
                table: "BuyerLinkages",
                column: "ReceivingBuyerId");

            migrationBuilder.CreateIndex(
                name: "IX_Buyers_BillingAddressId",
                table: "Buyers",
                column: "BillingAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_Buyers_ShippingAddressId",
                table: "Buyers",
                column: "ShippingAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_BuyerWishlistItems_BuyProductId",
                table: "BuyerWishlistItems",
                column: "BuyProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_OrderID",
                table: "Contracts",
                column: "OrderID");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_PDFID",
                table: "Contracts",
                column: "PDFID");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_PredefinedContractID",
                table: "Contracts",
                column: "PredefinedContractID");

            migrationBuilder.CreateIndex(
                name: "IX_DummyCards_BuyerId",
                table: "DummyCards",
                column: "BuyerId");

            migrationBuilder.CreateIndex(
                name: "IX_DummyCards_CardNumber",
                table: "DummyCards",
                column: "CardNumber");

            migrationBuilder.CreateIndex(
                name: "IX_DummyWallets_BuyerId",
                table: "DummyWallets",
                column: "BuyerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Followings_SellerId",
                table: "Followings",
                column: "SellerId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderCheckpoints_TrackedOrderID",
                table: "OrderCheckpoints",
                column: "TrackedOrderID");

            migrationBuilder.CreateIndex(
                name: "IX_OrderNotifications_ContractID",
                table: "OrderNotifications",
                column: "ContractID");

            migrationBuilder.CreateIndex(
                name: "IX_OrderNotifications_OrderID",
                table: "OrderNotifications",
                column: "OrderID");

            migrationBuilder.CreateIndex(
                name: "IX_OrderNotifications_ProductID",
                table: "OrderNotifications",
                column: "ProductID");

            migrationBuilder.CreateIndex(
                name: "IX_OrderNotifications_RecipientID",
                table: "OrderNotifications",
                column: "RecipientID");

            migrationBuilder.CreateIndex(
                name: "IX_SellerNotifications_SellerID",
                table: "SellerNotifications",
                column: "SellerID");

            migrationBuilder.CreateIndex(
                name: "IX_TrackedOrders_OrderID",
                table: "TrackedOrders",
                column: "OrderID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserWaitList_ProductWaitListID",
                table: "UserWaitList",
                column: "ProductWaitListID");

            migrationBuilder.CreateIndex(
                name: "IX_UserWaitList_UserID",
                table: "UserWaitList",
                column: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_AuctionProducts_ProductCategories_category_id",
                table: "AuctionProducts",
                column: "category_id",
                principalTable: "ProductCategories",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AuctionProducts_ProductConditions_condition_id",
                table: "AuctionProducts",
                column: "condition_id",
                principalTable: "ProductConditions",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BuyProducts_ProductCategories_category_id",
                table: "BuyProducts",
                column: "category_id",
                principalTable: "ProductCategories",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BuyProducts_ProductConditions_condition_id",
                table: "BuyProducts",
                column: "condition_id",
                principalTable: "ProductConditions",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_BuyProducts_BuyProductId",
                table: "Orders",
                column: "BuyProductId",
                principalTable: "BuyProducts",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Buyers_BuyerID",
                table: "Orders",
                column: "BuyerID",
                principalTable: "Buyers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_OrderHistory_OrderHistoryID",
                table: "Orders",
                column: "OrderHistoryID",
                principalTable: "OrderHistory",
                principalColumn: "OrderID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_OrderSummary_OrderSummaryID",
                table: "Orders",
                column: "OrderSummaryID",
                principalTable: "OrderSummary",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Users_SellerId",
                table: "Orders",
                column: "SellerId",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuctionProducts_ProductCategories_category_id",
                table: "AuctionProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_AuctionProducts_ProductConditions_condition_id",
                table: "AuctionProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_BuyProducts_ProductCategories_category_id",
                table: "BuyProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_BuyProducts_ProductConditions_condition_id",
                table: "BuyProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_BuyProducts_BuyProductId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Buyers_BuyerID",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_OrderHistory_OrderHistoryID",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_OrderSummary_OrderSummaryID",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Users_SellerId",
                table: "Orders");

            migrationBuilder.DropTable(
                name: "BuyerCartItems");

            migrationBuilder.DropTable(
                name: "BuyerLinkages");

            migrationBuilder.DropTable(
                name: "BuyerWishlistItems");

            migrationBuilder.DropTable(
                name: "DummyCards");

            migrationBuilder.DropTable(
                name: "DummyWallets");

            migrationBuilder.DropTable(
                name: "Followings");

            migrationBuilder.DropTable(
                name: "OrderCheckpoints");

            migrationBuilder.DropTable(
                name: "OrderHistory");

            migrationBuilder.DropTable(
                name: "OrderNotifications");

            migrationBuilder.DropTable(
                name: "OrderSummary");

            migrationBuilder.DropTable(
                name: "SellerNotifications");

            migrationBuilder.DropTable(
                name: "UserWaitList");

            migrationBuilder.DropTable(
                name: "TrackedOrders");

            migrationBuilder.DropTable(
                name: "Contracts");

            migrationBuilder.DropTable(
                name: "Sellers");

            migrationBuilder.DropTable(
                name: "Buyers");

            migrationBuilder.DropTable(
                name: "PDFs");

            migrationBuilder.DropTable(
                name: "PredefinedContracts");

            migrationBuilder.DropTable(
                name: "Addresses");

            migrationBuilder.DropIndex(
                name: "IX_Orders_BuyerID",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_BuyProductId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_OrderHistoryID",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_OrderSummaryID",
                table: "Orders");

            migrationBuilder.DropCheckConstraint(
                name: "PaymentMethodConstraint",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "bannedUntil",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "failedLogIns",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "isBanned",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "phoneNumber",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "score",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "BuyProductId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "OrderDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "OrderHistoryID",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "OrderSummaryID",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ProductID",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ProductType",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "stock",
                table: "BuyProducts");

            migrationBuilder.DropColumn(
                name: "price",
                table: "BorrowProducts");

            migrationBuilder.DropColumn(
                name: "stock",
                table: "BorrowProducts");

            migrationBuilder.DropColumn(
                name: "price",
                table: "AuctionProducts");

            migrationBuilder.DropColumn(
                name: "stock",
                table: "AuctionProducts");

            migrationBuilder.RenameColumn(
                name: "BuyerID",
                table: "Orders",
                newName: "BuyerId");

            migrationBuilder.AddColumn<double>(
                name: "rating",
                table: "Users",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "BuyProducts",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "condition_id",
                table: "BuyProducts",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "category_id",
                table: "BuyProducts",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "AuctionProducts",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "condition_id",
                table: "AuctionProducts",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "category_id",
                table: "AuctionProducts",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateTable(
                name: "BasketItemsBuyProducts",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    basket_id = table.Column<int>(type: "int", nullable: false),
                    price = table.Column<double>(type: "float", nullable: false),
                    product_id = table.Column<int>(type: "int", nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_BasketItemsBuyProducts_product_id",
                table: "BasketItemsBuyProducts",
                column: "product_id");

            migrationBuilder.AddForeignKey(
                name: "FK_AuctionProducts_ProductCategories_category_id",
                table: "AuctionProducts",
                column: "category_id",
                principalTable: "ProductCategories",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_AuctionProducts_ProductConditions_condition_id",
                table: "AuctionProducts",
                column: "condition_id",
                principalTable: "ProductConditions",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_BuyProducts_ProductCategories_category_id",
                table: "BuyProducts",
                column: "category_id",
                principalTable: "ProductCategories",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_BuyProducts_ProductConditions_condition_id",
                table: "BuyProducts",
                column: "condition_id",
                principalTable: "ProductConditions",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Users_SellerId",
                table: "Orders",
                column: "SellerId",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
