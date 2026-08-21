using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApexWorld_Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordResetFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ResetToken",
                schema: "REPMS",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResetTokenExpiry",
                schema: "REPMS",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                schema: "REPMS",
                table: "DashboardMetrics",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 18, 11, 50, 26, 495, DateTimeKind.Utc).AddTicks(1503), new DateTime(2026, 8, 18, 11, 50, 26, 495, DateTimeKind.Utc).AddTicks(1504) });

            migrationBuilder.UpdateData(
                schema: "REPMS",
                table: "DashboardMetrics",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 18, 11, 50, 26, 495, DateTimeKind.Utc).AddTicks(1512), new DateTime(2026, 8, 18, 11, 50, 26, 495, DateTimeKind.Utc).AddTicks(1512) });

            migrationBuilder.UpdateData(
                schema: "REPMS",
                table: "DashboardMetrics",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 18, 11, 50, 26, 495, DateTimeKind.Utc).AddTicks(1516), new DateTime(2026, 8, 18, 11, 50, 26, 495, DateTimeKind.Utc).AddTicks(1516) });

            migrationBuilder.UpdateData(
                schema: "REPMS",
                table: "DashboardMetrics",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 18, 11, 50, 26, 495, DateTimeKind.Utc).AddTicks(1520), new DateTime(2026, 8, 18, 11, 50, 26, 495, DateTimeKind.Utc).AddTicks(1520) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResetToken",
                schema: "REPMS",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ResetTokenExpiry",
                schema: "REPMS",
                table: "Users");

            migrationBuilder.UpdateData(
                schema: "REPMS",
                table: "DashboardMetrics",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 18, 7, 53, 47, 485, DateTimeKind.Utc).AddTicks(9914), new DateTime(2026, 8, 18, 7, 53, 47, 485, DateTimeKind.Utc).AddTicks(9915) });

            migrationBuilder.UpdateData(
                schema: "REPMS",
                table: "DashboardMetrics",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 18, 7, 53, 47, 485, DateTimeKind.Utc).AddTicks(9928), new DateTime(2026, 8, 18, 7, 53, 47, 485, DateTimeKind.Utc).AddTicks(9929) });

            migrationBuilder.UpdateData(
                schema: "REPMS",
                table: "DashboardMetrics",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 18, 7, 53, 47, 485, DateTimeKind.Utc).AddTicks(9935), new DateTime(2026, 8, 18, 7, 53, 47, 485, DateTimeKind.Utc).AddTicks(9936) });

            migrationBuilder.UpdateData(
                schema: "REPMS",
                table: "DashboardMetrics",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 18, 7, 53, 47, 485, DateTimeKind.Utc).AddTicks(9941), new DateTime(2026, 8, 18, 7, 53, 47, 485, DateTimeKind.Utc).AddTicks(9942) });
        }
    }
}
