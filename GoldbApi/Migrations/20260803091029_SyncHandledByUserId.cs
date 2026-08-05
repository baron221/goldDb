using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GoldbApi.Migrations
{
    /// <inheritdoc />
    public partial class SyncHandledByUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "is_deleted",
                schema: "goldb",
                table: "users",
                type: "boolean",
                nullable: false,
                comment: "삭제 여부",
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<decimal>(
                name: "actual_weight",
                schema: "goldb",
                table: "stocks",
                type: "numeric",
                nullable: false,
                comment: "실중량 (1개 기준)",
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldComment: "실중량");

            migrationBuilder.AddColumn<int>(
                name: "quantity",
                schema: "goldb",
                table: "stocks",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "수량");

            migrationBuilder.AddColumn<string>(
                name: "size",
                schema: "goldb",
                table: "stocks",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                comment: "사이즈");

            migrationBuilder.AddColumn<decimal>(
                name: "discount",
                schema: "goldb",
                table: "receivables",
                type: "numeric",
                nullable: false,
                defaultValue: 0m,
                comment: "할인 금액");

            migrationBuilder.AddColumn<bool>(
                name: "is_cancelled",
                schema: "goldb",
                table: "receivables",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                comment: "취소 여부");

            migrationBuilder.AddColumn<decimal>(
                name: "remaining_weight",
                schema: "goldb",
                table: "receivables",
                type: "numeric",
                nullable: false,
                defaultValue: 0m,
                comment: "잔여 순금 중량(g)");

            migrationBuilder.AddColumn<decimal>(
                name: "weight",
                schema: "goldb",
                table: "receivables",
                type: "numeric",
                nullable: false,
                defaultValue: 0m,
                comment: "순금 중량(g)");

            migrationBuilder.AddColumn<int>(
                name: "handled_by_user_id",
                schema: "goldb",
                table: "orders",
                type: "integer",
                nullable: true,
                comment: "담당자 ID (대리 주문을 처리한 직원)");

            migrationBuilder.AddColumn<string>(
                name: "memo",
                schema: "goldb",
                table: "order_items",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                comment: "주문 메모");

            migrationBuilder.AddColumn<string>(
                name: "size",
                schema: "goldb",
                table: "order_items",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                comment: "사이즈");

            migrationBuilder.AddColumn<string>(
                name: "memo",
                schema: "goldb",
                table: "cart_items",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                comment: "주문 메모");

            migrationBuilder.AddColumn<string>(
                name: "size",
                schema: "goldb",
                table: "cart_items",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                comment: "사이즈");

            migrationBuilder.CreateTable(
                name: "payables",
                schema: "goldb",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    logistics_company_id = table.Column<int>(type: "integer", nullable: false),
                    manufacturer_company_id = table.Column<int>(type: "integer", nullable: false),
                    order_id = table.Column<int>(type: "integer", nullable: true),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    remaining_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    weight = table.Column<decimal>(type: "numeric", nullable: false, comment: "순금 중량(g)"),
                    remaining_weight = table.Column<decimal>(type: "numeric", nullable: false, comment: "잔여 순금 중량(g)"),
                    memo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    settlement_method = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    discount = table.Column<decimal>(type: "numeric", nullable: false, comment: "할인 금액"),
                    is_cancelled = table.Column<bool>(type: "boolean", nullable: false, comment: "취소 여부"),
                    created_by = table.Column<int>(type: "integer", nullable: true, comment: "생성자 ID"),
                    updated_by = table.Column<int>(type: "integer", nullable: true, comment: "수정자 ID"),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "생성 일시"),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "수정 일시"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, comment: "삭제 여부")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payables", x => x.id);
                    table.ForeignKey(
                        name: "fk_payables_companies_logistics_company_id",
                        column: x => x.logistics_company_id,
                        principalSchema: "goldb",
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_payables_companies_manufacturer_company_id",
                        column: x => x.manufacturer_company_id,
                        principalSchema: "goldb",
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_payables_orders_order_id",
                        column: x => x.order_id,
                        principalSchema: "goldb",
                        principalTable: "orders",
                        principalColumn: "id");
                },
                comment: "물류-공장 정산 내역 (물류가 공장에 지급해야 할 금액)");

            migrationBuilder.CreateTable(
                name: "receivable_applications",
                schema: "goldb",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    deposit_id = table.Column<int>(type: "integer", nullable: false),
                    charge_id = table.Column<int>(type: "integer", nullable: false),
                    applied_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    applied_weight = table.Column<decimal>(type: "numeric", nullable: false, comment: "적용된 순금 중량(g)"),
                    created_by = table.Column<int>(type: "integer", nullable: true, comment: "생성자 ID"),
                    updated_by = table.Column<int>(type: "integer", nullable: true, comment: "수정자 ID"),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "생성 일시"),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "수정 일시"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, comment: "삭제 여부")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_receivable_applications", x => x.id);
                    table.ForeignKey(
                        name: "fk_receivable_applications_receivables_charge_id",
                        column: x => x.charge_id,
                        principalSchema: "goldb",
                        principalTable: "receivables",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_receivable_applications_receivables_deposit_id",
                        column: x => x.deposit_id,
                        principalSchema: "goldb",
                        principalTable: "receivables",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "입금이 어느 청구건에 얼마나 적용됐는지 기록 (정산 내역 추적용)");

            migrationBuilder.CreateIndex(
                name: "ix_orders_handled_by_user_id",
                schema: "goldb",
                table: "orders",
                column: "handled_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_payables_logistics_company_id",
                schema: "goldb",
                table: "payables",
                column: "logistics_company_id");

            migrationBuilder.CreateIndex(
                name: "ix_payables_manufacturer_company_id",
                schema: "goldb",
                table: "payables",
                column: "manufacturer_company_id");

            migrationBuilder.CreateIndex(
                name: "ix_payables_order_id",
                schema: "goldb",
                table: "payables",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_receivable_applications_charge_id",
                schema: "goldb",
                table: "receivable_applications",
                column: "charge_id");

            migrationBuilder.CreateIndex(
                name: "ix_receivable_applications_deposit_id",
                schema: "goldb",
                table: "receivable_applications",
                column: "deposit_id");

            migrationBuilder.AddForeignKey(
                name: "fk_orders_users_handled_by_user_id",
                schema: "goldb",
                table: "orders",
                column: "handled_by_user_id",
                principalSchema: "goldb",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_orders_users_handled_by_user_id",
                schema: "goldb",
                table: "orders");

            migrationBuilder.DropTable(
                name: "payables",
                schema: "goldb");

            migrationBuilder.DropTable(
                name: "receivable_applications",
                schema: "goldb");

            migrationBuilder.DropIndex(
                name: "ix_orders_handled_by_user_id",
                schema: "goldb",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "quantity",
                schema: "goldb",
                table: "stocks");

            migrationBuilder.DropColumn(
                name: "size",
                schema: "goldb",
                table: "stocks");

            migrationBuilder.DropColumn(
                name: "discount",
                schema: "goldb",
                table: "receivables");

            migrationBuilder.DropColumn(
                name: "is_cancelled",
                schema: "goldb",
                table: "receivables");

            migrationBuilder.DropColumn(
                name: "remaining_weight",
                schema: "goldb",
                table: "receivables");

            migrationBuilder.DropColumn(
                name: "weight",
                schema: "goldb",
                table: "receivables");

            migrationBuilder.DropColumn(
                name: "handled_by_user_id",
                schema: "goldb",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "memo",
                schema: "goldb",
                table: "order_items");

            migrationBuilder.DropColumn(
                name: "size",
                schema: "goldb",
                table: "order_items");

            migrationBuilder.DropColumn(
                name: "memo",
                schema: "goldb",
                table: "cart_items");

            migrationBuilder.DropColumn(
                name: "size",
                schema: "goldb",
                table: "cart_items");

            migrationBuilder.AlterColumn<bool>(
                name: "is_deleted",
                schema: "goldb",
                table: "users",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldComment: "삭제 여부");

            migrationBuilder.AlterColumn<decimal>(
                name: "actual_weight",
                schema: "goldb",
                table: "stocks",
                type: "numeric",
                nullable: false,
                comment: "실중량",
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldComment: "실중량 (1개 기준)");
        }
    }
}
