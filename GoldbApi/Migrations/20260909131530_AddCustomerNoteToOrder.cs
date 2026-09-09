using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldbApi.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerNoteToOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "customer_note_memo",
                schema: "goldb",
                table: "orders",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                comment: "소매점이 고객관리 주문내역에서 남긴 메모 (사진 첨부 가능)");

            migrationBuilder.AddColumn<string>(
                name: "customer_note_photo_url",
                schema: "goldb",
                table: "orders",
                type: "text",
                nullable: true,
                comment: "위 메모에 첨부된 사진 URL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "customer_note_memo",
                schema: "goldb",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "customer_note_photo_url",
                schema: "goldb",
                table: "orders");
        }
    }
}
