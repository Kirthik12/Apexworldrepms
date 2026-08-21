using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApexWorld_Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddPropertyIdToWishlist : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM [REPMS].[Wishlists];");

            migrationBuilder.AddColumn<int>(
                name: "PropertyId",
                schema: "REPMS",
                table: "Wishlists",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Wishlists_PropertyId",
                schema: "REPMS",
                table: "Wishlists",
                column: "PropertyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Wishlists_Properties_PropertyId",
                schema: "REPMS",
                table: "Wishlists",
                column: "PropertyId",
                principalSchema: "REPMS",
                principalTable: "Properties",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.DropTable(
                name: "WishlistItems",
                schema: "REPMS");
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Wishlists_Properties_PropertyId",
                schema: "REPMS",
                table: "Wishlists");

            migrationBuilder.DropIndex(
                name: "IX_Wishlists_PropertyId",
                schema: "REPMS",
                table: "Wishlists");

            migrationBuilder.DropColumn(
                name: "PropertyId",
                schema: "REPMS",
                table: "Wishlists");

            migrationBuilder.CreateTable(
                name: "WishlistItems",
                schema: "REPMS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    WishlistId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WishlistItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WishlistItems_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalSchema: "REPMS",
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WishlistItems_Wishlists_WishlistId",
                        column: x => x.WishlistId,
                        principalSchema: "REPMS",
                        principalTable: "Wishlists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WishlistItems_PropertyId",
                schema: "REPMS",
                table: "WishlistItems",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_WishlistItems_WishlistId",
                schema: "REPMS",
                table: "WishlistItems",
                column: "WishlistId");
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
        }
    }
}




