using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.DbContexts.Migrations
{
    /// <inheritdoc />
    public partial class AddDifficulties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lib_DetailedCompetitionStatuses_Lib_CompetitionsStatuses_CompetitionsStatusId",
                schema: "Library",
                table: "Lib_DetailedCompetitionStatuses");

            migrationBuilder.RenameColumn(
                name: "LongName",
                schema: "Library",
                table: "Lib_RefereeLevels",
                newName: "FullName");

            migrationBuilder.RenameColumn(
                name: "Difficulty",
                schema: "Distance",
                table: "Distance_SportEvents",
                newName: "DisciplineGroupId");

            migrationBuilder.AddColumn<int>(
                name: "DifficultyId",
                schema: "Distance",
                table: "Distance_SportEvents",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Lib_Difficulties",
                schema: "Library",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    DisciplineGroupId = table.Column<int>(type: "INTEGER", nullable: false),
                    FullName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    FullNameGenitive = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Difficulties", x => new { x.Id, x.DisciplineGroupId });
                    table.ForeignKey(
                        name: "FK_Difficulties_DisciplineGroupId",
                        column: x => x.DisciplineGroupId,
                        principalSchema: "Library",
                        principalTable: "Lib_DisciplineGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Трудности");

            migrationBuilder.CreateIndex(
                name: "IX_Distance_SportEvents_DifficultyId_DisciplineGroupId",
                schema: "Distance",
                table: "Distance_SportEvents",
                columns: new[] { "DifficultyId", "DisciplineGroupId" });

            migrationBuilder.CreateIndex(
                name: "IX_Lib_Difficulties_DisciplineGroupId",
                schema: "Library",
                table: "Lib_Difficulties",
                column: "DisciplineGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_SportEvents_DifficultyId_DisciplineGroupId",
                schema: "Distance",
                table: "Distance_SportEvents",
                columns: new[] { "DifficultyId", "DisciplineGroupId" },
                principalSchema: "Library",
                principalTable: "Lib_Difficulties",
                principalColumns: new[] { "Id", "DisciplineGroupId" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DetailedCompetitionStatuses_CompetitionsStatusId",
                schema: "Library",
                table: "Lib_DetailedCompetitionStatuses",
                column: "CompetitionsStatusId",
                principalSchema: "Library",
                principalTable: "Lib_CompetitionsStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SportEvents_DifficultyId_DisciplineGroupId",
                schema: "Distance",
                table: "Distance_SportEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_DetailedCompetitionStatuses_CompetitionsStatusId",
                schema: "Library",
                table: "Lib_DetailedCompetitionStatuses");

            migrationBuilder.DropTable(
                name: "Lib_Difficulties",
                schema: "Library");

            migrationBuilder.DropIndex(
                name: "IX_Distance_SportEvents_DifficultyId_DisciplineGroupId",
                schema: "Distance",
                table: "Distance_SportEvents");

            migrationBuilder.DropColumn(
                name: "DifficultyId",
                schema: "Distance",
                table: "Distance_SportEvents");

            migrationBuilder.RenameColumn(
                name: "FullName",
                schema: "Library",
                table: "Lib_RefereeLevels",
                newName: "LongName");

            migrationBuilder.RenameColumn(
                name: "DisciplineGroupId",
                schema: "Distance",
                table: "Distance_SportEvents",
                newName: "Difficulty");

            migrationBuilder.AddForeignKey(
                name: "FK_Lib_DetailedCompetitionStatuses_Lib_CompetitionsStatuses_CompetitionsStatusId",
                schema: "Library",
                table: "Lib_DetailedCompetitionStatuses",
                column: "CompetitionsStatusId",
                principalSchema: "Library",
                principalTable: "Lib_CompetitionsStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
