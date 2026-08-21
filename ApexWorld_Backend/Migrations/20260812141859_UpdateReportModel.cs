using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApexWorld_Backend.Migrations
{
    /// <inheritdoc />
    public partial class UpdateReportModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                schema: "REPMS",
                table: "Reports",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileUrl",
                schema: "REPMS",
                table: "Reports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Format",
                schema: "REPMS",
                table: "Reports",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GeneratedBy",
                schema: "REPMS",
                table: "Reports",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PropertyScope",
                schema: "REPMS",
                table: "Reports",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReportName",
                schema: "REPMS",
                table: "Reports",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                schema: "REPMS",
                table: "Reports",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                schema: "REPMS",
                table: "Reports",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");


        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndDate",
                schema: "REPMS",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "FileUrl",
                schema: "REPMS",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "Format",
                schema: "REPMS",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "GeneratedBy",
                schema: "REPMS",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "PropertyScope",
                schema: "REPMS",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "ReportName",
                schema: "REPMS",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "StartDate",
                schema: "REPMS",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "REPMS",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "Message",
                schema: "REPMS",
                table: "Enquiries");
        }
    }
}
