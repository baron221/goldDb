using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldbApi.Migrations
{
    /// <inheritdoc />
    public partial class AddIsOverdueSettlementToReceivable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_overdue_settlement",
                schema: "goldb",
                table: "receivables",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                comment: "미수금 관리에서 발생한 단순 수금 여부");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_overdue_settlement",
                schema: "goldb",
                table: "receivables");
        }
    }
}
