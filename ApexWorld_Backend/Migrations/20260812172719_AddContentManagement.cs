using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApexWorld_Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddContentManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Contents",
                schema: "REPMS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Section = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contents", x => x.Id);
                });

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Contents",
                schema: "REPMS");

            migrationBuilder.UpdateData(
                schema: "REPMS",
                table: "DashboardMetrics",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 12, 15, 8, 58, 396, DateTimeKind.Utc).AddTicks(5005), new DateTime(2026, 8, 12, 15, 8, 58, 396, DateTimeKind.Utc).AddTicks(5007) });

            migrationBuilder.UpdateData(
                schema: "REPMS",
                table: "DashboardMetrics",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 12, 15, 8, 58, 396, DateTimeKind.Utc).AddTicks(5023), new DateTime(2026, 8, 12, 15, 8, 58, 396, DateTimeKind.Utc).AddTicks(5024) });

            migrationBuilder.UpdateData(
                schema: "REPMS",
                table: "DashboardMetrics",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 12, 15, 8, 58, 396, DateTimeKind.Utc).AddTicks(5031), new DateTime(2026, 8, 12, 15, 8, 58, 396, DateTimeKind.Utc).AddTicks(5032) });

            migrationBuilder.UpdateData(
                schema: "REPMS",
                table: "DashboardMetrics",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 12, 15, 8, 58, 396, DateTimeKind.Utc).AddTicks(5038), new DateTime(2026, 8, 12, 15, 8, 58, 396, DateTimeKind.Utc).AddTicks(5039) });
        }
    }
}
