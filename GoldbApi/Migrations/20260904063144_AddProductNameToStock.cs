using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldbApi.Migrations
{
    /// <inheritdoc />
    public partial class AddProductNameToStock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "product_name",
                schema: "goldb",
                table: "stocks",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                comment: "직접 입력한 제품명 (카탈로그에 없는 경우)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "product_name",
                schema: "goldb",
                table: "stocks");
        }
    }
}
