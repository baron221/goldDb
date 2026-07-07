using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldbApi.Migrations
{
    /// <inheritdoc />
    public partial class AddUserApproval : Migration
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
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldComment: "삭제 여부");

            migrationBuilder.AddColumn<bool>(
                name: "is_approved",
                schema: "goldb",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_approved",
                schema: "goldb",
                table: "users");

            migrationBuilder.AlterColumn<bool>(
                name: "is_deleted",
                schema: "goldb",
                table: "users",
                type: "boolean",
                nullable: false,
                comment: "삭제 여부",
                oldClrType: typeof(bool),
                oldType: "boolean");
        }
    }
}
