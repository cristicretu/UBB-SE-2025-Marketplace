using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace server.Migrations
{
    /// <inheritdoc />
    public partial class FixTrackedOrderCheckpointStatusType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "OrderChekpointConstraint",
                table: "OrderCheckpoints");

            migrationBuilder.AddCheckConstraint(
                name: "OrderChekpointConstraint",
                table: "OrderCheckpoints",
                sql: "[Status] IN (0, 1, 2, 3, 4, 5)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "OrderChekpointConstraint",
                table: "OrderCheckpoints");

            migrationBuilder.AddCheckConstraint(
                name: "OrderChekpointConstraint",
                table: "OrderCheckpoints",
                sql: "[Status] IN ('PROCESSING', 'SHIPPED', 'IN_WAREHOUSE', 'IN_TRANSIT', 'OUT_FOR_DELIVERY', 'DELIVERED')");
        }
    }
}
