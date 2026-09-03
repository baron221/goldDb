using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldbApi.Migrations
{
    /// <inheritdoc />
    public partial class AddIsOverdueSettlementToPayable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_overdue_settlement",
                schema: "goldb",
                table: "payables",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                comment: "미수금 관리에서 발생한 단순 정산 여부");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_overdue_settlement",
                schema: "goldb",
                table: "payables");
        }
    }
}
