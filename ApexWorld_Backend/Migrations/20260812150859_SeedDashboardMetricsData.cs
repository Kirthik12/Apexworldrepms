using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ApexWorld_Backend.Migrations
{
    /// <inheritdoc />
    public partial class SeedDashboardMetricsData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "REPMS",
                table: "DashboardMetrics",
                columns: new[] { "Id", "Category", "CreatedAt", "DisplayName", "IsDeleted", "Key", "RowVersion", "Trend", "UpdatedAt", "Value" },
                values: new object[,]
                {
                    { 1, "Listings", new DateTime(2026, 8, 12, 15, 8, 58, 396, DateTimeKind.Utc).AddTicks(5005), "Active Listings", false, "ActiveListings", null, "up", new DateTime(2026, 8, 12, 15, 8, 58, 396, DateTimeKind.Utc).AddTicks(5007), 13m },
                    { 2, "Revenue", new DateTime(2026, 8, 12, 15, 8, 58, 396, DateTimeKind.Utc).AddTicks(5023), "Total Completed Revenue (Cr)", false, "TotalCompletedRevenue", null, "up", new DateTime(2026, 8, 12, 15, 8, 58, 396, DateTimeKind.Utc).AddTicks(5024), 5.79m },
                    { 3, "Loans", new DateTime(2026, 8, 12, 15, 8, 58, 396, DateTimeKind.Utc).AddTicks(5031), "Pending Loan Applications", false, "PendingLoanApplications", null, "stable", new DateTime(2026, 8, 12, 15, 8, 58, 396, DateTimeKind.Utc).AddTicks(5032), 5m },
                    { 4, "Enquiries", new DateTime(2026, 8, 12, 15, 8, 58, 396, DateTimeKind.Utc).AddTicks(5038), "Unresolved Enquiries", false, "UnresolvedEnquiries", null, "down", new DateTime(2026, 8, 12, 15, 8, 58, 396, DateTimeKind.Utc).AddTicks(5039), 4m }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "REPMS",
                table: "DashboardMetrics",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                schema: "REPMS",
                table: "DashboardMetrics",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                schema: "REPMS",
                table: "DashboardMetrics",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                schema: "REPMS",
                table: "DashboardMetrics",
                keyColumn: "Id",
                keyValue: 4);
        }
    }
}
