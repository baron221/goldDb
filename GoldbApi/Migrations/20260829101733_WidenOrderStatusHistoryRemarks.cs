using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldbApi.Migrations
{
    /// <inheritdoc />
    public partial class WidenOrderStatusHistoryRemarks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "remarks",
                schema: "goldb",
                table: "order_status_histories",
                type: "text",
                nullable: true,
                comment: "메모",
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true,
                oldComment: "메모");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "remarks",
                schema: "goldb",
                table: "order_status_histories",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                comment: "메모",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldComment: "메모");
        }
    }
}
