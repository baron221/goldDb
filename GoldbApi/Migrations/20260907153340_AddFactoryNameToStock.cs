using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldbApi.Migrations
{
    /// <inheritdoc />
    public partial class AddFactoryNameToStock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "factory_name",
                schema: "goldb",
                table: "stocks",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                comment: "직접 입력한 생산공장명 (카탈로그 제품과 연결되지 않은 경우)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "factory_name",
                schema: "goldb",
                table: "stocks");
        }
    }
}
