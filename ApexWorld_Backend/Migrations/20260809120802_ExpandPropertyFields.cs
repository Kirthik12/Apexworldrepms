using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApexWorld_Backend.Migrations
{
    /// <inheritdoc />
    public partial class ExpandPropertyFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                schema: "REPMS",
                table: "Properties",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "AreaSize",
                schema: "REPMS",
                table: "Properties",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Bathrooms",
                schema: "REPMS",
                table: "Properties",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Bedrooms",
                schema: "REPMS",
                table: "Properties",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CarParking",
                schema: "REPMS",
                table: "Properties",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CarpetArea",
                schema: "REPMS",
                table: "Properties",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Facing",
                schema: "REPMS",
                table: "Properties",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Furnishing",
                schema: "REPMS",
                table: "Properties",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Maintenance",
                schema: "REPMS",
                table: "Properties",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ProjectName",
                schema: "REPMS",
                table: "Properties",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TotalFloors",
                schema: "REPMS",
                table: "Properties",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                schema: "REPMS",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "AreaSize",
                schema: "REPMS",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "Bathrooms",
                schema: "REPMS",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "Bedrooms",
                schema: "REPMS",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "CarParking",
                schema: "REPMS",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "CarpetArea",
                schema: "REPMS",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "Facing",
                schema: "REPMS",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "Furnishing",
                schema: "REPMS",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "Maintenance",
                schema: "REPMS",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "ProjectName",
                schema: "REPMS",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "TotalFloors",
                schema: "REPMS",
                table: "Properties");
        }
    }
}
