using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GoldbApi.Migrations
{
    /// <inheritdoc />
    public partial class AddPayableApplication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "payable_applications",
                schema: "goldb",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    payment_id = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("pk_payable_applications", x => x.id);
                    table.ForeignKey(
                        name: "fk_payable_applications_payables_charge_id",
                        column: x => x.charge_id,
                        principalSchema: "goldb",
                        principalTable: "payables",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_payable_applications_payables_payment_id",
                        column: x => x.payment_id,
                        principalSchema: "goldb",
                        principalTable: "payables",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "정산 처리가 어느 청구건에 얼마나 적용됐는지 기록 (정산 내역 추적용)");

            migrationBuilder.CreateIndex(
                name: "ix_payable_applications_charge_id",
                schema: "goldb",
                table: "payable_applications",
                column: "charge_id");

            migrationBuilder.CreateIndex(
                name: "ix_payable_applications_payment_id",
                schema: "goldb",
                table: "payable_applications",
                column: "payment_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "payable_applications",
                schema: "goldb");
        }
    }
}
