using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldbApi.Migrations
{
    /// <inheritdoc />
    public partial class AddMfgProcessedToPayable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_mfg_processed",
                schema: "goldb",
                table: "payables",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                comment: "공장 정산 처리 여부 (실제 입금 확인이 아닌 공장측 확인 표시)");

            migrationBuilder.AddColumn<DateTime>(
                name: "mfg_processed_at",
                schema: "goldb",
                table: "payables",
                type: "timestamp without time zone",
                nullable: true,
                comment: "공장 정산 처리 일시");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_mfg_processed",
                schema: "goldb",
                table: "payables");

            migrationBuilder.DropColumn(
                name: "mfg_processed_at",
                schema: "goldb",
                table: "payables");
        }
    }
}
