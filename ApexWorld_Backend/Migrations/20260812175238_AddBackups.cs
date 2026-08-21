using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApexWorld_Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddBackups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Backups",
                schema: "REPMS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BackupName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataIncluded = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ScheduleType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateAndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Size = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Backups", x => x.Id);
                });

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Backups",
                schema: "REPMS");

            migrationBuilder.UpdateData(
                schema: "REPMS",
                table: "DashboardMetrics",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 12, 17, 27, 19, 155, DateTimeKind.Utc).AddTicks(7841), new DateTime(2026, 8, 12, 17, 27, 19, 155, DateTimeKind.Utc).AddTicks(7845) });

            migrationBuilder.UpdateData(
                schema: "REPMS",
                table: "DashboardMetrics",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 12, 17, 27, 19, 155, DateTimeKind.Utc).AddTicks(7858), new DateTime(2026, 8, 12, 17, 27, 19, 155, DateTimeKind.Utc).AddTicks(7859) });

            migrationBuilder.UpdateData(
                schema: "REPMS",
                table: "DashboardMetrics",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 12, 17, 27, 19, 155, DateTimeKind.Utc).AddTicks(7864), new DateTime(2026, 8, 12, 17, 27, 19, 155, DateTimeKind.Utc).AddTicks(7865) });

            migrationBuilder.UpdateData(
                schema: "REPMS",
                table: "DashboardMetrics",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 12, 17, 27, 19, 155, DateTimeKind.Utc).AddTicks(7869), new DateTime(2026, 8, 12, 17, 27, 19, 155, DateTimeKind.Utc).AddTicks(7870) });
        }
    }
}
