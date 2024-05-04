using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.DbContexts.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Common");

            migrationBuilder.EnsureSchema(
                name: "Distance");

            migrationBuilder.EnsureSchema(
                name: "Library");

            migrationBuilder.CreateTable(
                name: "Lib_CompetitionsStatuses",
                schema: "Library",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    NamePlural = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompetitionsStatuses", x => x.Id);
                },
                comment: "Статусы соревнований");

            migrationBuilder.CreateTable(
                name: "Lib_DisciplineGroups",
                schema: "Library",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DisciplineGroups", x => x.Id);
                },
                comment: "Группы дисциплин");

            migrationBuilder.CreateTable(
                name: "Lib_RefereeJobTitles",
                schema: "Library",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefereeJobTitles", x => x.Id);
                },
                comment: "Судейские должности");

            migrationBuilder.CreateTable(
                name: "Lib_RefereeLevels",
                schema: "Library",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    LongName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefereeLevels", x => x.Id);
                },
                comment: "Судейские категории");

            migrationBuilder.CreateTable(
                name: "Lib_Sexes",
                schema: "Library",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    PersonalityName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    PersonalityNamePlural = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    TeamName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    TeamNamePlural = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sexes", x => x.Id);
                },
                comment: "Варианты пола");

            migrationBuilder.CreateTable(
                name: "Lib_SportUnitTypes",
                schema: "Library",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    AuxName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SportUnitTypes", x => x.Id);
                },
                comment: "Типы спортивных юнитов");

            migrationBuilder.CreateTable(
                name: "Lib_DetailedCompetitionStatuses",
                schema: "Library",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    CompetitionsStatusId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetailedCompetitionStatuses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Lib_DetailedCompetitionStatuses_Lib_CompetitionsStatuses_CompetitionsStatusId",
                        column: x => x.CompetitionsStatusId,
                        principalSchema: "Library",
                        principalTable: "Lib_CompetitionsStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Статусы и наименования соревнований");

            migrationBuilder.CreateTable(
                name: "Lib_DisciplineSubGroups",
                schema: "Library",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    DisciplineGroupId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DisciplineSubGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DisciplineSubGroups_DisciplineGroupId",
                        column: x => x.DisciplineGroupId,
                        principalSchema: "Library",
                        principalTable: "Lib_DisciplineGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Подгруппы дисциплин");

            migrationBuilder.CreateTable(
                name: "Common_Referees",
                schema: "Common",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Domicile = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    RefereeLevelId = table.Column<int>(type: "INTEGER", nullable: false),
                    RefereeRefereeJobTitleId = table.Column<int>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Patronymic = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: false, computedColumnSql: "LastName || ' ' || FirstName || IIF(Patronymic IS NULL, '', ' ' || Patronymic)", stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Referees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Referees_RefereeLevelId",
                        column: x => x.RefereeLevelId,
                        principalSchema: "Library",
                        principalTable: "Lib_RefereeLevels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Referees_RefereeingPositionId",
                        column: x => x.RefereeRefereeJobTitleId,
                        principalSchema: "Library",
                        principalTable: "Lib_RefereeJobTitles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Судьи");

            migrationBuilder.CreateTable(
                name: "Common_Representatives",
                schema: "Common",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PhoneNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    SexId = table.Column<int>(type: "INTEGER", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Patronymic = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: false, computedColumnSql: "LastName || ' ' || FirstName || IIF(Patronymic IS NULL, '', ' ' || Patronymic)", stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Representatives", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Representatives_SexId",
                        column: x => x.SexId,
                        principalSchema: "Library",
                        principalTable: "Lib_Sexes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Представители");

            migrationBuilder.CreateTable(
                name: "Common_CompetitionData",
                schema: "Common",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ConductingOrganizations = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DayCount = table.Column<int>(type: "INTEGER", nullable: false),
                    Venue = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    CompetitionsStatusId = table.Column<int>(type: "INTEGER", nullable: false),
                    DetailedCompetitionStatusId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Competitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SportEvents_CompetitionsStatusId",
                        column: x => x.CompetitionsStatusId,
                        principalSchema: "Library",
                        principalTable: "Lib_CompetitionsStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SportEvents_DetailedCompetitionStatusId",
                        column: x => x.DetailedCompetitionStatusId,
                        principalSchema: "Library",
                        principalTable: "Lib_DetailedCompetitionStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Данные о соревновании");

            migrationBuilder.CreateTable(
                name: "Lib_Disciplines",
                schema: "Library",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    DisciplineSubGroupId = table.Column<int>(type: "INTEGER", nullable: false),
                    DisciplineGroupId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Disciplines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Disciplines_DisciplineGroupId",
                        column: x => x.DisciplineGroupId,
                        principalSchema: "Library",
                        principalTable: "Lib_DisciplineGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Disciplines_DisciplineSubGroupId",
                        column: x => x.DisciplineSubGroupId,
                        principalSchema: "Library",
                        principalTable: "Lib_DisciplineSubGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Дисциплины");

            migrationBuilder.CreateTable(
                name: "Common_Delegations",
                schema: "Common",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Number = table.Column<int>(type: "INTEGER", nullable: false),
                    Region = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    RepresentativeId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Delegations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Delegations_RepresentativeId",
                        column: x => x.RepresentativeId,
                        principalSchema: "Common",
                        principalTable: "Common_Representatives",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Делегации");

            migrationBuilder.CreateTable(
                name: "Distance_SportEvents",
                schema: "Distance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IsShort = table.Column<bool>(type: "INTEGER", nullable: true),
                    Difficulty = table.Column<int>(type: "INTEGER", nullable: false),
                    DisciplineId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SportEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SportEvents_DisciplineId",
                        column: x => x.DisciplineId,
                        principalSchema: "Library",
                        principalTable: "Lib_Disciplines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Виды программ");

            migrationBuilder.CreateTable(
                name: "Distance_SportUnits",
                schema: "Distance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SexId = table.Column<int>(type: "INTEGER", nullable: false),
                    SportUnitTypeId = table.Column<int>(type: "INTEGER", nullable: false),
                    SportEventId = table.Column<int>(type: "INTEGER", nullable: false),
                    ParentSportUnitId = table.Column<int>(type: "INTEGER", nullable: true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SportUnits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SportUnits_ParentSportUnitId",
                        column: x => x.ParentSportUnitId,
                        principalSchema: "Distance",
                        principalTable: "Distance_SportUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SportUnits_SexId",
                        column: x => x.SexId,
                        principalSchema: "Library",
                        principalTable: "Lib_Sexes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SportUnits_SportEventId",
                        column: x => x.SportEventId,
                        principalSchema: "Distance",
                        principalTable: "Distance_SportEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SportUnits_SportUnitTypeId",
                        column: x => x.SportUnitTypeId,
                        principalSchema: "Library",
                        principalTable: "Lib_SportUnitTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Спортивные юниты");

            migrationBuilder.CreateTable(
                name: "Common_Athletes",
                schema: "Common",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BirthDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SexId = table.Column<int>(type: "INTEGER", nullable: false),
                    DelegationId = table.Column<int>(type: "INTEGER", nullable: false),
                    SportUnitId = table.Column<int>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Patronymic = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: false, computedColumnSql: "LastName || ' ' || FirstName || IIF(Patronymic IS NULL, '', ' ' || Patronymic)", stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Athletes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Athletes_DelegationId",
                        column: x => x.DelegationId,
                        principalSchema: "Common",
                        principalTable: "Common_Delegations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Athletes_SexId",
                        column: x => x.SexId,
                        principalSchema: "Library",
                        principalTable: "Lib_Sexes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Athletes_SportUnitId",
                        column: x => x.SportUnitId,
                        principalSchema: "Distance",
                        principalTable: "Distance_SportUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Спортсмены");

            migrationBuilder.CreateIndex(
                name: "IX_Common_Athletes_DelegationId",
                schema: "Common",
                table: "Common_Athletes",
                column: "DelegationId");

            migrationBuilder.CreateIndex(
                name: "IX_Common_Athletes_SexId",
                schema: "Common",
                table: "Common_Athletes",
                column: "SexId");

            migrationBuilder.CreateIndex(
                name: "IX_Common_Athletes_SportUnitId",
                schema: "Common",
                table: "Common_Athletes",
                column: "SportUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Common_CompetitionData_CompetitionsStatusId",
                schema: "Common",
                table: "Common_CompetitionData",
                column: "CompetitionsStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Common_CompetitionData_DetailedCompetitionStatusId",
                schema: "Common",
                table: "Common_CompetitionData",
                column: "DetailedCompetitionStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Common_Delegations_RepresentativeId",
                schema: "Common",
                table: "Common_Delegations",
                column: "RepresentativeId");

            migrationBuilder.CreateIndex(
                name: "IX_Common_Referees_RefereeLevelId",
                schema: "Common",
                table: "Common_Referees",
                column: "RefereeLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_Common_Referees_RefereeRefereeJobTitleId",
                schema: "Common",
                table: "Common_Referees",
                column: "RefereeRefereeJobTitleId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Common_Representatives_SexId",
                schema: "Common",
                table: "Common_Representatives",
                column: "SexId");

            migrationBuilder.CreateIndex(
                name: "IX_Distance_SportEvents_DisciplineId",
                schema: "Distance",
                table: "Distance_SportEvents",
                column: "DisciplineId");

            migrationBuilder.CreateIndex(
                name: "IX_Distance_SportUnits_ParentSportUnitId",
                schema: "Distance",
                table: "Distance_SportUnits",
                column: "ParentSportUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Distance_SportUnits_SexId",
                schema: "Distance",
                table: "Distance_SportUnits",
                column: "SexId");

            migrationBuilder.CreateIndex(
                name: "IX_Distance_SportUnits_SportEventId",
                schema: "Distance",
                table: "Distance_SportUnits",
                column: "SportEventId");

            migrationBuilder.CreateIndex(
                name: "IX_Distance_SportUnits_SportUnitTypeId",
                schema: "Distance",
                table: "Distance_SportUnits",
                column: "SportUnitTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Lib_DetailedCompetitionStatuses_CompetitionsStatusId",
                schema: "Library",
                table: "Lib_DetailedCompetitionStatuses",
                column: "CompetitionsStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Lib_Disciplines_DisciplineGroupId",
                schema: "Library",
                table: "Lib_Disciplines",
                column: "DisciplineGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Lib_Disciplines_DisciplineSubGroupId",
                schema: "Library",
                table: "Lib_Disciplines",
                column: "DisciplineSubGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Lib_DisciplineSubGroups_DisciplineGroupId",
                schema: "Library",
                table: "Lib_DisciplineSubGroups",
                column: "DisciplineGroupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Common_Athletes",
                schema: "Common");

            migrationBuilder.DropTable(
                name: "Common_CompetitionData",
                schema: "Common");

            migrationBuilder.DropTable(
                name: "Common_Referees",
                schema: "Common");

            migrationBuilder.DropTable(
                name: "Common_Delegations",
                schema: "Common");

            migrationBuilder.DropTable(
                name: "Distance_SportUnits",
                schema: "Distance");

            migrationBuilder.DropTable(
                name: "Lib_DetailedCompetitionStatuses",
                schema: "Library");

            migrationBuilder.DropTable(
                name: "Lib_RefereeLevels",
                schema: "Library");

            migrationBuilder.DropTable(
                name: "Lib_RefereeJobTitles",
                schema: "Library");

            migrationBuilder.DropTable(
                name: "Common_Representatives",
                schema: "Common");

            migrationBuilder.DropTable(
                name: "Distance_SportEvents",
                schema: "Distance");

            migrationBuilder.DropTable(
                name: "Lib_SportUnitTypes",
                schema: "Library");

            migrationBuilder.DropTable(
                name: "Lib_CompetitionsStatuses",
                schema: "Library");

            migrationBuilder.DropTable(
                name: "Lib_Sexes",
                schema: "Library");

            migrationBuilder.DropTable(
                name: "Lib_Disciplines",
                schema: "Library");

            migrationBuilder.DropTable(
                name: "Lib_DisciplineSubGroups",
                schema: "Library");

            migrationBuilder.DropTable(
                name: "Lib_DisciplineGroups",
                schema: "Library");
        }
    }
}
