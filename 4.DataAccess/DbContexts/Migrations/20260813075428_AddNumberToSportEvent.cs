using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.DbContexts.Migrations
{
    /// <inheritdoc />
    public partial class AddNumberToSportEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Number",
                schema: "Distance",
                table: "Distance_SportEvents",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Number",
                schema: "Distance",
                table: "Distance_SportEvents");
        }
    }
}
