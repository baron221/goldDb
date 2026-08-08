using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldbApi.Migrations
{
    /// <inheritdoc />
    public partial class AddHidePriceToCompany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "hide_price",
                schema: "goldb",
                table: "companies",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                comment: "소매점 가격 표시 숨김 여부");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "hide_price",
                schema: "goldb",
                table: "companies");
        }
    }
}
