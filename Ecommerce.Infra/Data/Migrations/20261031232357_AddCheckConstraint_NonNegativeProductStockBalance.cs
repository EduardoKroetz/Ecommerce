using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCheckConstraint_NonNegativeProductStockBalance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_Products_StockBalance_NonNegative",
                table: "Products",
                sql: "\"StockBalance\" >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Products_StockBalance_NonNegative",
                table: "Products");
        }
    }
}
