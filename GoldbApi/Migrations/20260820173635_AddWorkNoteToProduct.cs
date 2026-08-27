using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldbApi.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkNoteToProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "work_note",
                schema: "goldb",
                table: "products",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                comment: "작업내용 (공장 전용 메모)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "work_note",
                schema: "goldb",
                table: "products");
        }
    }
}
