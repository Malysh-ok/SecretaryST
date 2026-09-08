using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.DbContexts.Migrations
{
    /// <inheritdoc />
    public partial class AddRefereeRoleAvailabilityEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Lib_RefereeRoleAvailabilities",
                schema: "Library",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    DetailedCompetitionStatusId = table.Column<int>(type: "INTEGER", nullable: false),
                    RefereeCategoryId = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxRefereesPerRole = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefereeRoles", x => new { x.Id, x.DetailedCompetitionStatusId });
                },
                comment: "Доступности судейских должностей");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Lib_RefereeRoleAvailabilities",
                schema: "Library");
        }
    }
}
