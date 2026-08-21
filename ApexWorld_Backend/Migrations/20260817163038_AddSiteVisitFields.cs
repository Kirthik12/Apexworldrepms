using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApexWorld_Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddSiteVisitFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InterestOutcome",
                schema: "REPMS",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsVisited",
                schema: "REPMS",
                table: "Bookings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                schema: "REPMS",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "VisitedDate",
                schema: "REPMS",
                table: "Bookings",
                type: "datetime2",
                nullable: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InterestOutcome",
                schema: "REPMS",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "IsVisited",
                schema: "REPMS",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                schema: "REPMS",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "VisitedDate",
                schema: "REPMS",
                table: "Bookings");

            migrationBuilder.UpdateData(
                schema: "REPMS",
                table: "DashboardMetrics",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 17, 11, 57, 35, 885, DateTimeKind.Utc).AddTicks(8205), new DateTime(2026, 8, 17, 11, 57, 35, 885, DateTimeKind.Utc).AddTicks(8205) });

            migrationBuilder.UpdateData(
                schema: "REPMS",
                table: "DashboardMetrics",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 17, 11, 57, 35, 885, DateTimeKind.Utc).AddTicks(8213), new DateTime(2026, 8, 17, 11, 57, 35, 885, DateTimeKind.Utc).AddTicks(8214) });

            migrationBuilder.UpdateData(
                schema: "REPMS",
                table: "DashboardMetrics",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 17, 11, 57, 35, 885, DateTimeKind.Utc).AddTicks(8216), new DateTime(2026, 8, 17, 11, 57, 35, 885, DateTimeKind.Utc).AddTicks(8217) });

            migrationBuilder.UpdateData(
                schema: "REPMS",
                table: "DashboardMetrics",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 17, 11, 57, 35, 885, DateTimeKind.Utc).AddTicks(8219), new DateTime(2026, 8, 17, 11, 57, 35, 885, DateTimeKind.Utc).AddTicks(8220) });
        }
    }
}
