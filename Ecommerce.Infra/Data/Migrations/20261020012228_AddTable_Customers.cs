using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Ecommerce.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTable_Customers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Customers: one per user (ADR 0012)
            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Customers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Customers_UserId",
                table: "Customers",
                column: "UserId",
                unique: true);

            // 2. Backfill: every existing user (sellers and the legacy seller included) gets its customer.
            // On an empty database this inserts nothing.
            migrationBuilder.Sql("""
                INSERT INTO "Customers" ("UserId")
                SELECT "Id" FROM "AspNetUsers";
                """);

            // 3. Expand: new columns start nullable
            migrationBuilder.AddColumn<int>(
                name: "CustomerId",
                table: "Purchases",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CustomerId",
                table: "Carts",
                type: "integer",
                nullable: true);

            // 4. Fill from the current UserId
            migrationBuilder.Sql("""
                UPDATE "Purchases" p SET "CustomerId" = c."Id"
                FROM "Customers" c
                WHERE c."UserId" = p."UserId";
                """);

            migrationBuilder.Sql("""
                UPDATE "Carts" ca SET "CustomerId" = c."Id"
                FROM "Customers" c
                WHERE c."UserId" = ca."UserId";
                """);

            // 5. Require the columns, then index and constrain them
            migrationBuilder.AlterColumn<int>(
                name: "CustomerId",
                table: "Purchases",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CustomerId",
                table: "Carts",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_CustomerId",
                table: "Purchases",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Carts_CustomerId",
                table: "Carts",
                column: "CustomerId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Purchases_Customers_CustomerId",
                table: "Purchases",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Carts_Customers_CustomerId",
                table: "Carts",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // 6. Contract: drop the old UserId columns
            migrationBuilder.DropForeignKey(
                name: "FK_Purchases_AspNetUsers_UserId",
                table: "Purchases");

            migrationBuilder.DropForeignKey(
                name: "FK_Carts_AspNetUsers_UserId",
                table: "Carts");

            migrationBuilder.DropIndex(
                name: "IX_Purchases_UserId",
                table: "Purchases");

            migrationBuilder.DropIndex(
                name: "IX_Carts_UserId",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Purchases");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Carts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Mirror of Up: restore UserId from Customers before dropping them
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Purchases",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Carts",
                type: "text",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE "Purchases" p SET "UserId" = c."UserId"
                FROM "Customers" c
                WHERE c."Id" = p."CustomerId";
                """);

            migrationBuilder.Sql("""
                UPDATE "Carts" ca SET "UserId" = c."UserId"
                FROM "Customers" c
                WHERE c."Id" = ca."CustomerId";
                """);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Purchases",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Carts",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_UserId",
                table: "Purchases",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Carts_UserId",
                table: "Carts",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Purchases_AspNetUsers_UserId",
                table: "Purchases",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Carts_AspNetUsers_UserId",
                table: "Carts",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.DropForeignKey(
                name: "FK_Purchases_Customers_CustomerId",
                table: "Purchases");

            migrationBuilder.DropForeignKey(
                name: "FK_Carts_Customers_CustomerId",
                table: "Carts");

            migrationBuilder.DropIndex(
                name: "IX_Purchases_CustomerId",
                table: "Purchases");

            migrationBuilder.DropIndex(
                name: "IX_Carts_CustomerId",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "Purchases");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "Carts");

            migrationBuilder.DropTable(
                name: "Customers");
        }
    }
}
