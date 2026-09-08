using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.DbContexts.Migrations
{
    /// <inheritdoc />
    public partial class RenameCompetitionEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Delegations_CompetitionDataId",
                schema: "Common",
                table: "Common_Delegations");

            migrationBuilder.DropForeignKey(
                name: "FK_Referees_CompetitionDataId",
                schema: "Common",
                table: "Common_Referees");

            migrationBuilder.DropForeignKey(
                name: "FK_SportEvents_CompetitionDataId",
                schema: "Distance",
                table: "Distance_SportEvents");

            migrationBuilder.DropTable(
                name: "Common_CompetitionData",
                schema: "Common");

            migrationBuilder.RenameColumn(
                name: "CompetitionDataId",
                schema: "Distance",
                table: "Distance_SportEvents",
                newName: "CompetitionId");

            migrationBuilder.RenameIndex(
                name: "IX_Distance_SportEvents_CompetitionDataId",
                schema: "Distance",
                table: "Distance_SportEvents",
                newName: "IX_Distance_SportEvents_CompetitionId");

            migrationBuilder.RenameColumn(
                name: "CompetitionDataId",
                schema: "Common",
                table: "Common_Referees",
                newName: "CompetitionId");

            migrationBuilder.RenameIndex(
                name: "IX_Common_Referees_CompetitionDataId",
                schema: "Common",
                table: "Common_Referees",
                newName: "IX_Common_Referees_CompetitionId");

            migrationBuilder.RenameColumn(
                name: "CompetitionDataId",
                schema: "Common",
                table: "Common_Delegations",
                newName: "CompetitionId");

            migrationBuilder.RenameIndex(
                name: "IX_Common_Delegations_CompetitionDataId",
                schema: "Common",
                table: "Common_Delegations",
                newName: "IX_Common_Delegations_CompetitionId");

            migrationBuilder.CreateTable(
                name: "Common_Competitions",
                schema: "Common",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ConductingOrganizations = table.Column<string>(type: "json", nullable: false),
                    InitialDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Venue = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    ShortName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    IsStudentCompetition = table.Column<bool>(type: "INTEGER", nullable: false),
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

            migrationBuilder.CreateIndex(
                name: "IX_Common_Competitions_CompetitionsStatusId",
                schema: "Common",
                table: "Common_Competitions",
                column: "CompetitionsStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Common_Competitions_DetailedCompetitionStatusId",
                schema: "Common",
                table: "Common_Competitions",
                column: "DetailedCompetitionStatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_Delegations_CompetitionId",
                schema: "Common",
                table: "Common_Delegations",
                column: "CompetitionId",
                principalSchema: "Common",
                principalTable: "Common_Competitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Referees_CompetitionId",
                schema: "Common",
                table: "Common_Referees",
                column: "CompetitionId",
                principalSchema: "Common",
                principalTable: "Common_Competitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SportEvents_CompetitionId",
                schema: "Distance",
                table: "Distance_SportEvents",
                column: "CompetitionId",
                principalSchema: "Common",
                principalTable: "Common_Competitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Delegations_CompetitionId",
                schema: "Common",
                table: "Common_Delegations");

            migrationBuilder.DropForeignKey(
                name: "FK_Referees_CompetitionId",
                schema: "Common",
                table: "Common_Referees");

            migrationBuilder.DropForeignKey(
                name: "FK_SportEvents_CompetitionId",
                schema: "Distance",
                table: "Distance_SportEvents");

            migrationBuilder.DropTable(
                name: "Common_Competitions",
                schema: "Common");

            migrationBuilder.RenameColumn(
                name: "CompetitionId",
                schema: "Distance",
                table: "Distance_SportEvents",
                newName: "CompetitionDataId");

            migrationBuilder.RenameIndex(
                name: "IX_Distance_SportEvents_CompetitionId",
                schema: "Distance",
                table: "Distance_SportEvents",
                newName: "IX_Distance_SportEvents_CompetitionDataId");

            migrationBuilder.RenameColumn(
                name: "CompetitionId",
                schema: "Common",
                table: "Common_Referees",
                newName: "CompetitionDataId");

            migrationBuilder.RenameIndex(
                name: "IX_Common_Referees_CompetitionId",
                schema: "Common",
                table: "Common_Referees",
                newName: "IX_Common_Referees_CompetitionDataId");

            migrationBuilder.RenameColumn(
                name: "CompetitionId",
                schema: "Common",
                table: "Common_Delegations",
                newName: "CompetitionDataId");

            migrationBuilder.RenameIndex(
                name: "IX_Common_Delegations_CompetitionId",
                schema: "Common",
                table: "Common_Delegations",
                newName: "IX_Common_Delegations_CompetitionDataId");

            migrationBuilder.CreateTable(
                name: "Common_CompetitionData",
                schema: "Common",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CompetitionsStatusId = table.Column<int>(type: "INTEGER", nullable: false),
                    DetailedCompetitionStatusId = table.Column<int>(type: "INTEGER", nullable: false),
                    ConductingOrganizations = table.Column<string>(type: "json", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    InitialDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsStudentCompetition = table.Column<bool>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    ShortName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Venue = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false)
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

            migrationBuilder.AddForeignKey(
                name: "FK_Delegations_CompetitionDataId",
                schema: "Common",
                table: "Common_Delegations",
                column: "CompetitionDataId",
                principalSchema: "Common",
                principalTable: "Common_CompetitionData",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Referees_CompetitionDataId",
                schema: "Common",
                table: "Common_Referees",
                column: "CompetitionDataId",
                principalSchema: "Common",
                principalTable: "Common_CompetitionData",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SportEvents_CompetitionDataId",
                schema: "Distance",
                table: "Distance_SportEvents",
                column: "CompetitionDataId",
                principalSchema: "Common",
                principalTable: "Common_CompetitionData",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
