using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldbApi.Migrations
{
    /// <inheritdoc />
    public partial class AddProductNoToStock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "product_no",
                schema: "goldb",
                table: "stocks",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                comment: "직접 입력한 제품번호 (카탈로그 제품과 연결되지 않은 경우)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "product_no",
                schema: "goldb",
                table: "stocks");
        }
    }
}
