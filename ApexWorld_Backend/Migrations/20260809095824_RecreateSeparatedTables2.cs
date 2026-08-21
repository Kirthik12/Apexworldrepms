using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApexWorld_Backend.Migrations
{
    /// <inheritdoc />
    public partial class RecreateSeparatedTables2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentRecords_Bookings_BookingId",
                schema: "REPMS",
                table: "PaymentRecords");

            migrationBuilder.DropIndex(
                name: "IX_Properties_Category",
                schema: "REPMS",
                table: "Properties");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PaymentRecords",
                schema: "REPMS",
                table: "PaymentRecords");

            migrationBuilder.DropColumn(
                name: "BuyerId",
                schema: "REPMS",
                table: "WishlistItems");

            migrationBuilder.DropColumn(
                name: "AccountStatus",
                schema: "REPMS",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "BuyerAccountId",
                schema: "REPMS",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CreditScore",
                schema: "REPMS",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PanCardKycStatus",
                schema: "REPMS",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RefreshToken",
                schema: "REPMS",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RefreshTokenCount",
                schema: "REPMS",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RefreshTokenExpiryTime",
                schema: "REPMS",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Category",
                schema: "REPMS",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "BuyerName",
                schema: "REPMS",
                table: "PaymentRecords");

            migrationBuilder.RenameTable(
                name: "PaymentRecords",
                schema: "REPMS",
                newName: "Payments",
                newSchema: "REPMS");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentRecords_BookingId",
                schema: "REPMS",
                table: "Payments",
                newName: "IX_Payments_BookingId");

            migrationBuilder.AddColumn<int>(
                name: "WishlistId",
                schema: "REPMS",
                table: "WishlistItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                schema: "REPMS",
                table: "Properties",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Payments",
                schema: "REPMS",
                table: "Payments",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Admins",
                schema: "REPMS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Department = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Admins_Users_Id",
                        column: x => x.Id,
                        principalSchema: "REPMS",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Buyers",
                schema: "REPMS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    BuyerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PanCardKycStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreditScore = table.Column<int>(type: "int", nullable: true),
                    AccountStatus = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Buyers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Buyers_Users_Id",
                        column: x => x.Id,
                        principalSchema: "REPMS",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EMIPlans",
                schema: "REPMS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoanApplicationId = table.Column<int>(type: "int", nullable: false),
                    InstallmentAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Months = table.Column<int>(type: "int", nullable: false),
                    InterestRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EMIPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EMIPlans_LoanApplications_LoanApplicationId",
                        column: x => x.LoanApplicationId,
                        principalSchema: "REPMS",
                        principalTable: "LoanApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentHistory",
                schema: "REPMS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentId = table.Column<int>(type: "int", nullable: false),
                    StatusChange = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaymentRecordId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentHistory_Payments_PaymentRecordId",
                        column: x => x.PaymentRecordId,
                        principalSchema: "REPMS",
                        principalTable: "Payments",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Policies",
                schema: "REPMS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PolicyName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Terms = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Policies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PropertyCategories",
                schema: "REPMS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PropertyImages",
                schema: "REPMS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyImages_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalSchema: "REPMS",
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Receipts",
                schema: "REPMS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentId = table.Column<int>(type: "int", nullable: false),
                    ReceiptNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IssuedTo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Receipts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Receipts_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalSchema: "REPMS",
                        principalTable: "Payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                schema: "REPMS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExpiryTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "REPMS",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reports",
                schema: "REPMS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReportType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataPayload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Wishlists",
                schema: "REPMS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BuyerId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wishlists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Wishlists_Buyers_BuyerId",
                        column: x => x.BuyerId,
                        principalSchema: "REPMS",
                        principalTable: "Buyers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WishlistItems_WishlistId",
                schema: "REPMS",
                table: "WishlistItems",
                column: "WishlistId");

            migrationBuilder.CreateIndex(
                name: "IX_Properties_CategoryId",
                schema: "REPMS",
                table: "Properties",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_EMIPlans_LoanApplicationId",
                schema: "REPMS",
                table: "EMIPlans",
                column: "LoanApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentHistory_PaymentRecordId",
                schema: "REPMS",
                table: "PaymentHistory",
                column: "PaymentRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyImages_PropertyId",
                schema: "REPMS",
                table: "PropertyImages",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_PaymentId",
                schema: "REPMS",
                table: "Receipts",
                column: "PaymentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                schema: "REPMS",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Wishlists_BuyerId",
                schema: "REPMS",
                table: "Wishlists",
                column: "BuyerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Bookings_BookingId",
                schema: "REPMS",
                table: "Payments",
                column: "BookingId",
                principalSchema: "REPMS",
                principalTable: "Bookings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Properties_PropertyCategories_CategoryId",
                schema: "REPMS",
                table: "Properties",
                column: "CategoryId",
                principalSchema: "REPMS",
                principalTable: "PropertyCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WishlistItems_Wishlists_WishlistId",
                schema: "REPMS",
                table: "WishlistItems",
                column: "WishlistId",
                principalSchema: "REPMS",
                principalTable: "Wishlists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Bookings_BookingId",
                schema: "REPMS",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Properties_PropertyCategories_CategoryId",
                schema: "REPMS",
                table: "Properties");

            migrationBuilder.DropForeignKey(
                name: "FK_WishlistItems_Wishlists_WishlistId",
                schema: "REPMS",
                table: "WishlistItems");

            migrationBuilder.DropTable(
                name: "Admins",
                schema: "REPMS");

            migrationBuilder.DropTable(
                name: "EMIPlans",
                schema: "REPMS");

            migrationBuilder.DropTable(
                name: "PaymentHistory",
                schema: "REPMS");

            migrationBuilder.DropTable(
                name: "Policies",
                schema: "REPMS");

            migrationBuilder.DropTable(
                name: "PropertyCategories",
                schema: "REPMS");

            migrationBuilder.DropTable(
                name: "PropertyImages",
                schema: "REPMS");

            migrationBuilder.DropTable(
                name: "Receipts",
                schema: "REPMS");

            migrationBuilder.DropTable(
                name: "RefreshTokens",
                schema: "REPMS");

            migrationBuilder.DropTable(
                name: "Reports",
                schema: "REPMS");

            migrationBuilder.DropTable(
                name: "Wishlists",
                schema: "REPMS");

            migrationBuilder.DropTable(
                name: "Buyers",
                schema: "REPMS");

            migrationBuilder.DropIndex(
                name: "IX_WishlistItems_WishlistId",
                schema: "REPMS",
                table: "WishlistItems");

            migrationBuilder.DropIndex(
                name: "IX_Properties_CategoryId",
                schema: "REPMS",
                table: "Properties");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Payments",
                schema: "REPMS",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "WishlistId",
                schema: "REPMS",
                table: "WishlistItems");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                schema: "REPMS",
                table: "Properties");

            migrationBuilder.RenameTable(
                name: "Payments",
                schema: "REPMS",
                newName: "PaymentRecords",
                newSchema: "REPMS");

            migrationBuilder.RenameIndex(
                name: "IX_Payments_BookingId",
                schema: "REPMS",
                table: "PaymentRecords",
                newName: "IX_PaymentRecords_BookingId");

            migrationBuilder.AddColumn<string>(
                name: "BuyerId",
                schema: "REPMS",
                table: "WishlistItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AccountStatus",
                schema: "REPMS",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BuyerAccountId",
                schema: "REPMS",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreditScore",
                schema: "REPMS",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PanCardKycStatus",
                schema: "REPMS",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RefreshToken",
                schema: "REPMS",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RefreshTokenCount",
                schema: "REPMS",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "RefreshTokenExpiryTime",
                schema: "REPMS",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Category",
                schema: "REPMS",
                table: "Properties",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BuyerName",
                schema: "REPMS",
                table: "PaymentRecords",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PaymentRecords",
                schema: "REPMS",
                table: "PaymentRecords",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Properties_Category",
                schema: "REPMS",
                table: "Properties",
                column: "Category");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentRecords_Bookings_BookingId",
                schema: "REPMS",
                table: "PaymentRecords",
                column: "BookingId",
                principalSchema: "REPMS",
                principalTable: "Bookings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
