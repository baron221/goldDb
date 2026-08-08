using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldbApi.Migrations
{
    /// <inheritdoc />
    public partial class AddDiscountWeightToPayable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "discount_weight",
                schema: "goldb",
                table: "payables",
                type: "numeric",
                nullable: false,
                defaultValue: 0m,
                comment: "할인 순금 중량(g)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "discount_weight",
                schema: "goldb",
                table: "payables");
        }
    }
}
