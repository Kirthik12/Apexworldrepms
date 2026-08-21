using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApexWorld_Backend.Migrations
{
    /// <inheritdoc />
    public partial class EnhanceBookingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                schema: "REPMS",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                schema: "REPMS",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                schema: "REPMS",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PermanentAddress",
                schema: "REPMS",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                schema: "REPMS",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PurposeOfPurchase",
                schema: "REPMS",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TimeSlot",
                schema: "REPMS",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                schema: "REPMS",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "FirstName",
                schema: "REPMS",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "LastName",
                schema: "REPMS",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "PermanentAddress",
                schema: "REPMS",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                schema: "REPMS",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "PurposeOfPurchase",
                schema: "REPMS",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "TimeSlot",
                schema: "REPMS",
                table: "Bookings");
        }
    }
}
