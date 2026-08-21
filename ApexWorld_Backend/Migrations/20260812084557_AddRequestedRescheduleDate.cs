using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApexWorld_Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddRequestedRescheduleDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "RequestedRescheduleDate",
                schema: "REPMS",
                table: "Bookings",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequestedRescheduleDate",
                schema: "REPMS",
                table: "Bookings");
        }
    }
}
