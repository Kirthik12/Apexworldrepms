using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApexWorld_Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddIsActiveToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "REPMS",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                schema: "REPMS",
                table: "DashboardMetrics",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 17, 6, 48, 56, 283, DateTimeKind.Utc).AddTicks(5563), new DateTime(2026, 8, 17, 6, 48, 56, 283, DateTimeKind.Utc).AddTicks(5564) });

            migrationBuilder.UpdateData(
                schema: "REPMS",
                table: "DashboardMetrics",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 17, 6, 48, 56, 283, DateTimeKind.Utc).AddTicks(5577), new DateTime(2026, 8, 17, 6, 48, 56, 283, DateTimeKind.Utc).AddTicks(5578) });

            migrationBuilder.UpdateData(
                schema: "REPMS",
                table: "DashboardMetrics",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 17, 6, 48, 56, 283, DateTimeKind.Utc).AddTicks(5584), new DateTime(2026, 8, 17, 6, 48, 56, 283, DateTimeKind.Utc).AddTicks(5585) });

            migrationBuilder.UpdateData(
                schema: "REPMS",
                table: "DashboardMetrics",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 17, 6, 48, 56, 283, DateTimeKind.Utc).AddTicks(5589), new DateTime(2026, 8, 17, 6, 48, 56, 283, DateTimeKind.Utc).AddTicks(5590) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "REPMS",
                table: "Users");

            migrationBuilder.UpdateData(
                schema: "REPMS",
                table: "DashboardMetrics",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 12, 17, 52, 37, 717, DateTimeKind.Utc).AddTicks(2422), new DateTime(2026, 8, 12, 17, 52, 37, 717, DateTimeKind.Utc).AddTicks(2424) });

            migrationBuilder.UpdateData(
                schema: "REPMS",
                table: "DashboardMetrics",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 12, 17, 52, 37, 717, DateTimeKind.Utc).AddTicks(2437), new DateTime(2026, 8, 12, 17, 52, 37, 717, DateTimeKind.Utc).AddTicks(2438) });

            migrationBuilder.UpdateData(
                schema: "REPMS",
                table: "DashboardMetrics",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 12, 17, 52, 37, 717, DateTimeKind.Utc).AddTicks(2443), new DateTime(2026, 8, 12, 17, 52, 37, 717, DateTimeKind.Utc).AddTicks(2444) });

            migrationBuilder.UpdateData(
                schema: "REPMS",
                table: "DashboardMetrics",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 12, 17, 52, 37, 717, DateTimeKind.Utc).AddTicks(2452), new DateTime(2026, 8, 12, 17, 52, 37, 717, DateTimeKind.Utc).AddTicks(2453) });
        }
    }
}
