using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace server.Migrations
{
    /// <inheritdoc />
    public partial class FixTrackedOrderStatusType : Migration
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

            migrationBuilder.AddCheckConstraint(
                name: "OrderChekpointConstraint",
                table: "OrderCheckpoints",
                sql: "[Status] IN ('PROCESSING', 'SHIPPED', 'IN_WAREHOUSE', 'IN_TRANSIT', 'OUT_FOR_DELIVERY', 'DELIVERED')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "OrderChekpointConstraint",
                table: "OrderCheckpoints");

            migrationBuilder.AddCheckConstraint(
                name: "TrackedOrderConstraint",
                table: "TrackedOrders",
                sql: "[CurrentStatus] IN ('Processing', 'Shipped', 'InWarehouse', 'InTransit', 'OutForDelivery', 'Delivered')");

            migrationBuilder.AddCheckConstraint(
                name: "OrderChekpointConstraint",
                table: "OrderCheckpoints",
                sql: "[Status] IN ('Processing', 'Shipped', 'InWarehouse', 'InTransit', 'OutForDelivery', 'Delivered')");
        }
    }
}
