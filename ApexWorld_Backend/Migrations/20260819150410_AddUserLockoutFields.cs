using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApexWorld_Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddUserLockoutFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FailedLoginAttempts",
                schema: "REPMS",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LockoutEnd",
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
                values: new object[] { new DateTime(2026, 8, 19, 15, 4, 9, 176, DateTimeKind.Utc).AddTicks(4235), new DateTime(2026, 8, 19, 15, 4, 9, 176, DateTimeKind.Utc).AddTicks(4236) });

            migrationBuilder.UpdateData(
                schema: "REPMS",
                table: "DashboardMetrics",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 19, 15, 4, 9, 176, DateTimeKind.Utc).AddTicks(4244), new DateTime(2026, 8, 19, 15, 4, 9, 176, DateTimeKind.Utc).AddTicks(4245) });

            migrationBuilder.UpdateData(
                schema: "REPMS",
                table: "DashboardMetrics",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 19, 15, 4, 9, 176, DateTimeKind.Utc).AddTicks(4249), new DateTime(2026, 8, 19, 15, 4, 9, 176, DateTimeKind.Utc).AddTicks(4249) });

            migrationBuilder.UpdateData(
                schema: "REPMS",
                table: "DashboardMetrics",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 19, 15, 4, 9, 176, DateTimeKind.Utc).AddTicks(4253), new DateTime(2026, 8, 19, 15, 4, 9, 176, DateTimeKind.Utc).AddTicks(4253) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FailedLoginAttempts",
                schema: "REPMS",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LockoutEnd",
                schema: "REPMS",
                table: "Users");

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
    }
}
