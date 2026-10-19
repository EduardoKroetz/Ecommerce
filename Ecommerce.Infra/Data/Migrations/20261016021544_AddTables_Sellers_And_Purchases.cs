using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Ecommerce.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTables_Sellers_And_Purchases : Migration
    {
        private const string LegacySellerName = "Legacy Seller";
        private const string LegacySellerEmail = "legacy-seller@ecommerce.local";
        private const string LegacySellerNormalizedEmail = "LEGACY-SELLER@ECOMMERCE.LOCAL";

        private const string LegacySellerIdQuery = $"""
            SELECT s."Id" FROM "Sellers" s
            JOIN "AspNetUsers" u ON u."Id" = s."UserId"
            WHERE u."NormalizedEmail" = '{LegacySellerNormalizedEmail}'
            """;

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. New tables
            migrationBuilder.CreateTable(
                name: "Sellers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sellers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sellers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Sellers_UserId",
                table: "Sellers",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateTable(
                name: "Purchases",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    OriginalTotalAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    PaymentMethod = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Purchases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Purchases_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_UserId",
                table: "Purchases",
                column: "UserId");

            // 2. Legacy seller: only created when there are existing products to backfill.
            // The user has no password hash, so it cannot sign in with a password.
            migrationBuilder.Sql($"""
                INSERT INTO "AspNetUsers" ("Id", "Name", "UserName", "NormalizedUserName", "Email", "NormalizedEmail",
                    "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "PhoneNumber",
                    "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnd", "LockoutEnabled", "AccessFailedCount")
                SELECT gen_random_uuid()::text, '{LegacySellerName}', '{LegacySellerEmail}', '{LegacySellerNormalizedEmail}',
                    '{LegacySellerEmail}', '{LegacySellerNormalizedEmail}', FALSE, NULL,
                    upper(replace(gen_random_uuid()::text, '-', '')), gen_random_uuid()::text, NULL,
                    FALSE, FALSE, NULL, TRUE, 0
                WHERE EXISTS (SELECT 1 FROM "Products");
                """);

            migrationBuilder.Sql($"""
                INSERT INTO "Sellers" ("UserId")
                SELECT "Id" FROM "AspNetUsers" WHERE "NormalizedEmail" = '{LegacySellerNormalizedEmail}';
                """);

            // 3. Products.SellerId: add nullable, backfill with the legacy seller, then require it
            migrationBuilder.AddColumn<int>(
                name: "SellerId",
                table: "Products",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql($"""
                UPDATE "Products" SET "SellerId" = ({LegacySellerIdQuery});
                """);

            migrationBuilder.AlterColumn<int>(
                name: "SellerId",
                table: "Products",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_SellerId",
                table: "Products",
                column: "SellerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Sellers_SellerId",
                table: "Products",
                column: "SellerId",
                principalTable: "Sellers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // 4. Orders: one legacy purchase per existing order (same Id), legacy seller and status conversion.
            // Old EOrderStatus: Created = 0, Cancelled = 1.
            // New EOrderStatus: AwaitingPayment = 0, Cancelled = 4. EPurchaseStatus: Pending = 0, Cancelled = 2. EPaymentMethod: PIX = 0.
            migrationBuilder.AddColumn<int>(
                name: "PurchaseId",
                table: "Orders",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SellerId",
                table: "Orders",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql("""
                INSERT INTO "Purchases" ("Id", "UserId", "OriginalTotalAmount", "TotalAmount", "PaymentMethod", "Status", "CreatedAt")
                SELECT o."Id", o."UserId", o."TotalAmount",
                    CASE WHEN o."Status" = 1 THEN 0 ELSE o."TotalAmount" END,
                    0,
                    CASE WHEN o."Status" = 1 THEN 2 ELSE 0 END,
                    o."CreatedAt"
                FROM "Orders" o;
                """);

            migrationBuilder.Sql("""
                SELECT setval(pg_get_serial_sequence('"Purchases"', 'Id'), COALESCE((SELECT MAX("Id") FROM "Purchases"), 0) + 1, false);
                """);

            migrationBuilder.Sql($"""
                UPDATE "Orders" SET
                    "PurchaseId" = "Id",
                    "SellerId" = ({LegacySellerIdQuery}),
                    "Status" = CASE WHEN "Status" = 1 THEN 4 ELSE "Status" END;
                """);

            migrationBuilder.AlterColumn<int>(
                name: "PurchaseId",
                table: "Orders",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SellerId",
                table: "Orders",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_PurchaseId",
                table: "Orders",
                column: "PurchaseId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_SellerId",
                table: "Orders",
                column: "SellerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Purchases_PurchaseId",
                table: "Orders",
                column: "PurchaseId",
                principalTable: "Purchases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Sellers_SellerId",
                table: "Orders",
                column: "SellerId",
                principalTable: "Sellers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // 5. The buyer now lives on the purchase
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Orders");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Purchases_PurchaseId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Sellers_SellerId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Sellers_SellerId",
                table: "Products");

            migrationBuilder.DropTable(
                name: "Purchases");

            migrationBuilder.DropTable(
                name: "Sellers");

            migrationBuilder.DropIndex(
                name: "IX_Products_SellerId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Orders_PurchaseId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_SellerId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "SellerId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "PurchaseId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "SellerId",
                table: "Orders");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Orders",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
