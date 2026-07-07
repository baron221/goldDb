using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GoldbApi.Migrations
{
    /// <inheritdoc />
    public partial class AddManufacturerLogistics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "manufacturer_logistics",
                schema: "goldb",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, comment: "ID")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    manufacturer_id = table.Column<int>(type: "integer", nullable: false, comment: "생산업체 ID"),
                    logistics_id = table.Column<int>(type: "integer", nullable: false, comment: "물류센터 ID"),
                    created_by = table.Column<int>(type: "integer", nullable: true, comment: "생성자 ID"),
                    updated_by = table.Column<int>(type: "integer", nullable: true, comment: "수정자 ID"),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "생성 일시"),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "수정 일시"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, comment: "삭제 여부")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_manufacturer_logistics", x => x.id);
                    table.ForeignKey(
                        name: "fk_manufacturer_logistics_companies_logistics_id",
                        column: x => x.logistics_id,
                        principalSchema: "goldb",
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_manufacturer_logistics_companies_manufacturer_id",
                        column: x => x.manufacturer_id,
                        principalSchema: "goldb",
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "생산업체-물류센터 매핑");

            migrationBuilder.CreateIndex(
                name: "ix_manufacturer_logistics_logistics_id",
                schema: "goldb",
                table: "manufacturer_logistics",
                column: "logistics_id");

            migrationBuilder.CreateIndex(
                name: "ix_manufacturer_logistics_manufacturer_id",
                schema: "goldb",
                table: "manufacturer_logistics",
                column: "manufacturer_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "manufacturer_logistics",
                schema: "goldb");
        }
    }
}
