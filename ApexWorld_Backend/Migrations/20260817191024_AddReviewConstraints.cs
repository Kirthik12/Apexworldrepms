using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApexWorld_Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddReviewConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "REPMS",
                table: "DashboardMetrics",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 17, 19, 10, 23, 548, DateTimeKind.Utc).AddTicks(4711), new DateTime(2026, 8, 17, 19, 10, 23, 548, DateTimeKind.Utc).AddTicks(4713) });

            migrationBuilder.UpdateData(
                schema: "REPMS",
                table: "DashboardMetrics",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 17, 19, 10, 23, 548, DateTimeKind.Utc).AddTicks(4727), new DateTime(2026, 8, 17, 19, 10, 23, 548, DateTimeKind.Utc).AddTicks(4728) });

            migrationBuilder.UpdateData(
                schema: "REPMS",
                table: "DashboardMetrics",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 17, 19, 10, 23, 548, DateTimeKind.Utc).AddTicks(4735), new DateTime(2026, 8, 17, 19, 10, 23, 548, DateTimeKind.Utc).AddTicks(4736) });

            migrationBuilder.UpdateData(
                schema: "REPMS",
                table: "DashboardMetrics",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 17, 19, 10, 23, 548, DateTimeKind.Utc).AddTicks(4742), new DateTime(2026, 8, 17, 19, 10, 23, 548, DateTimeKind.Utc).AddTicks(4743) });

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_BuyerId_PropertyId",
                schema: "REPMS",
                table: "Reviews",
                columns: new[] { "BuyerId", "PropertyId" },
                unique: true,
                filter: "[ReviewType] = 'Property'");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Users_BuyerId",
                schema: "REPMS",
                table: "Reviews",
                column: "BuyerId",
                principalSchema: "REPMS",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Users_BuyerId",
                schema: "REPMS",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_BuyerId_PropertyId",
                schema: "REPMS",
                table: "Reviews");

            migrationBuilder.UpdateData(
                schema: "REPMS",
                table: "DashboardMetrics",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 17, 16, 30, 37, 330, DateTimeKind.Utc).AddTicks(7773), new DateTime(2026, 8, 17, 16, 30, 37, 330, DateTimeKind.Utc).AddTicks(7774) });

            migrationBuilder.UpdateData(
                schema: "REPMS",
                table: "DashboardMetrics",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 17, 16, 30, 37, 330, DateTimeKind.Utc).AddTicks(7782), new DateTime(2026, 8, 17, 16, 30, 37, 330, DateTimeKind.Utc).AddTicks(7783) });

            migrationBuilder.UpdateData(
                schema: "REPMS",
                table: "DashboardMetrics",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 17, 16, 30, 37, 330, DateTimeKind.Utc).AddTicks(7786), new DateTime(2026, 8, 17, 16, 30, 37, 330, DateTimeKind.Utc).AddTicks(7786) });

            migrationBuilder.UpdateData(
                schema: "REPMS",
                table: "DashboardMetrics",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 17, 16, 30, 37, 330, DateTimeKind.Utc).AddTicks(7789), new DateTime(2026, 8, 17, 16, 30, 37, 330, DateTimeKind.Utc).AddTicks(7789) });
        }
    }
}
