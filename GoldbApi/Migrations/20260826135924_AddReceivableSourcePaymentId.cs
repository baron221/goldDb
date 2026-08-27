using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldbApi.Migrations
{
    /// <inheritdoc />
    public partial class AddReceivableSourcePaymentId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "source_payment_id",
                schema: "goldb",
                table: "receivables",
                type: "integer",
                nullable: true,
                comment: "이 입금을 발생시킨 Payable(정산처리) 결제 ID - MFG-DCC 정산이 이 주문의 미수금에 비례 반영된 경우에만 값이 있음");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "source_payment_id",
                schema: "goldb",
                table: "receivables");
        }
    }
}
