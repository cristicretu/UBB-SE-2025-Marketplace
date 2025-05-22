using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace server.Migrations
{
    /// <inheritdoc />
    public partial class OrderHistoryFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "TrackedOrderConstraint",
                table: "TrackedOrders");

            migrationBuilder.DropCheckConstraint(
                name: "OrderChekpointConstraint",
                table: "OrderCheckpoints");

            migrationBuilder.AddColumn<int>(
                name: "BuyerID",
                table: "OrderHistory",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "OrderHistory",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "OrderHistory",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                table: "OrderHistory",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingAddress",
                table: "OrderHistory",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "price",
                table: "BorrowProducts",
                type: "float",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<double>(
                name: "price",
                table: "AuctionProducts",
                type: "float",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddCheckConstraint(
                name: "TrackedOrderConstraint",
                table: "TrackedOrders",
                sql: "[CurrentStatus] IN ('Processing', 'Shipped', 'InWarehouse', 'InTransit', 'OutForDelivery', 'Delivered')");

            migrationBuilder.AddCheckConstraint(
                name: "OrderChekpointConstraint",
                table: "OrderCheckpoints",
                sql: "[Status] IN ('Processing', 'Shipped', 'InWarehouse', 'InTransit', 'OutForDelivery', 'Delivered')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "TrackedOrderConstraint",
                table: "TrackedOrders");

            migrationBuilder.DropCheckConstraint(
                name: "OrderChekpointConstraint",
                table: "OrderCheckpoints");

            migrationBuilder.DropColumn(
                name: "BuyerID",
                table: "OrderHistory");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "OrderHistory");

            migrationBuilder.DropColumn(
                name: "Note",
                table: "OrderHistory");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "OrderHistory");

            migrationBuilder.DropColumn(
                name: "ShippingAddress",
                table: "OrderHistory");

            migrationBuilder.AlterColumn<int>(
                name: "price",
                table: "BorrowProducts",
                type: "int",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<int>(
                name: "price",
                table: "AuctionProducts",
                type: "int",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AddCheckConstraint(
                name: "TrackedOrderConstraint",
                table: "TrackedOrders",
                sql: "[CurrentStatus] IN ('PROCESSING', 'SHIPPED', 'IN_WAREHOUSE', 'IN_TRANSIT', 'OUT_FOR_DELIVERY', 'DELIVERED')");

            migrationBuilder.AddCheckConstraint(
                name: "OrderChekpointConstraint",
                table: "OrderCheckpoints",
                sql: "[Status] IN ('PROCESSING', 'SHIPPED', 'IN_WAREHOUSE', 'IN_TRANSIT', 'OUT_FOR_DELIVERY', 'DELIVERED')");
        }
    }
}
