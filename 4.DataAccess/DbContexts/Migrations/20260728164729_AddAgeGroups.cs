using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.DbContexts.Migrations
{
    /// <inheritdoc />
    public partial class AddAgeGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                schema: "Common",
                table: "Common_Representatives");

            migrationBuilder.DropColumn(
                name: "Name",
                schema: "Common",
                table: "Common_Referees");

            migrationBuilder.DropColumn(
                name: "Name",
                schema: "Common",
                table: "Common_Athletes");

            migrationBuilder.AddColumn<int>(
                name: "AgeGroupId",
                schema: "Distance",
                table: "Distance_SportEvents",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DisciplineSubGroupId",
                schema: "Distance",
                table: "Distance_SportEvents",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsStudentCompetition",
                schema: "Common",
                table: "Common_CompetitionData",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Lib_AgeGroups",
                schema: "Library",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    DisciplineSubGroupId = table.Column<int>(type: "INTEGER", nullable: false),
                    MinAge = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxAge = table.Column<int>(type: "INTEGER", nullable: true),
                    MinStudentAge = table.Column<int>(type: "INTEGER", nullable: true),
                    MaxStudentAge = table.Column<int>(type: "INTEGER", nullable: true),
                    IsStudentCompetition = table.Column<bool>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Difficulties", x => new { x.Id, x.DisciplineSubGroupId });
                    table.ForeignKey(
                        name: "FK_Difficulties_DisciplineSubGroupId",
                        column: x => x.DisciplineSubGroupId,
                        principalSchema: "Library",
                        principalTable: "Lib_DisciplineSubGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Возрастные группы");

            migrationBuilder.CreateIndex(
                name: "IX_Distance_SportEvents_AgeGroupId_DisciplineSubGroupId",
                schema: "Distance",
                table: "Distance_SportEvents",
                columns: new[] { "AgeGroupId", "DisciplineSubGroupId" });

            migrationBuilder.CreateIndex(
                name: "IX_Lib_AgeGroups_DisciplineSubGroupId",
                schema: "Library",
                table: "Lib_AgeGroups",
                column: "DisciplineSubGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_SportEvents_AgeGroupId_DisciplineSubGroupId",
                schema: "Distance",
                table: "Distance_SportEvents",
                columns: new[] { "AgeGroupId", "DisciplineSubGroupId" },
                principalSchema: "Library",
                principalTable: "Lib_AgeGroups",
                principalColumns: new[] { "Id", "DisciplineSubGroupId" },
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SportEvents_AgeGroupId_DisciplineSubGroupId",
                schema: "Distance",
                table: "Distance_SportEvents");

            migrationBuilder.DropTable(
                name: "Lib_AgeGroups",
                schema: "Library");

            migrationBuilder.DropIndex(
                name: "IX_Distance_SportEvents_AgeGroupId_DisciplineSubGroupId",
                schema: "Distance",
                table: "Distance_SportEvents");

            migrationBuilder.DropColumn(
                name: "AgeGroupId",
                schema: "Distance",
                table: "Distance_SportEvents");

            migrationBuilder.DropColumn(
                name: "DisciplineSubGroupId",
                schema: "Distance",
                table: "Distance_SportEvents");

            migrationBuilder.DropColumn(
                name: "IsStudentCompetition",
                schema: "Common",
                table: "Common_CompetitionData");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                schema: "Common",
                table: "Common_Representatives",
                type: "TEXT",
                nullable: false,
                computedColumnSql: "LastName || ' ' || FirstName || IIF(Patronymic IS NULL, '', ' ' || Patronymic)",
                stored: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                schema: "Common",
                table: "Common_Referees",
                type: "TEXT",
                nullable: false,
                computedColumnSql: "LastName || ' ' || FirstName || IIF(Patronymic IS NULL, '', ' ' || Patronymic)",
                stored: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                schema: "Common",
                table: "Common_Athletes",
                type: "TEXT",
                nullable: false,
                computedColumnSql: "LastName || ' ' || FirstName || IIF(Patronymic IS NULL, '', ' ' || Patronymic)",
                stored: true);
        }
    }
}
