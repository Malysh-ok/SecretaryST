using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.DbContexts.Migrations
{
    /// <inheritdoc />
    public partial class AddFullnameToDiscipline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FullName",
                schema: "Library",
                table: "Lib_Disciplines",
                type: "TEXT",
                maxLength: 150,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FullName",
                schema: "Library",
                table: "Lib_Disciplines");
        }
    }
}
