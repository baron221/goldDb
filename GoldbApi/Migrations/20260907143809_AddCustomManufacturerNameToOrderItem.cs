using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldbApi.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomManufacturerNameToOrderItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "custom_manufacturer_company_id",
                schema: "goldb",
                table: "order_items",
                type: "integer",
                nullable: true,
                comment: "수기 등록 청구 대상 회사 ID (DCC 자기 자신 - placeholder)",
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true,
                oldComment: "수기 등록 제조사 회사 ID");

            migrationBuilder.AddColumn<string>(
                name: "custom_manufacturer_name",
                schema: "goldb",
                table: "order_items",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                comment: "수기 입력 제조사명");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "custom_manufacturer_name",
                schema: "goldb",
                table: "order_items");

            migrationBuilder.AlterColumn<int>(
                name: "custom_manufacturer_company_id",
                schema: "goldb",
                table: "order_items",
                type: "integer",
                nullable: true,
                comment: "수기 등록 제조사 회사 ID",
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true,
                oldComment: "수기 등록 청구 대상 회사 ID (DCC 자기 자신 - placeholder)");
        }
    }
}
