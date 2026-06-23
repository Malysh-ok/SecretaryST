using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.DbContexts.Migrations
{
    /// <inheritdoc />
    public partial class MadeRefereeSportEventDelegationDependentOnCompetitionData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompetitionDataId",
                schema: "Distance",
                table: "Distance_SportEvents",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CompetitionDataId",
                schema: "Common",
                table: "Common_Referees",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CompetitionDataId",
                schema: "Common",
                table: "Common_Delegations",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Distance_SportEvents_CompetitionDataId",
                schema: "Distance",
                table: "Distance_SportEvents",
                column: "CompetitionDataId");

            migrationBuilder.CreateIndex(
                name: "IX_Common_Referees_CompetitionDataId",
                schema: "Common",
                table: "Common_Referees",
                column: "CompetitionDataId");

            migrationBuilder.CreateIndex(
                name: "IX_Common_Delegations_CompetitionDataId",
                schema: "Common",
                table: "Common_Delegations",
                column: "CompetitionDataId");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropIndex(
                name: "IX_Distance_SportEvents_CompetitionDataId",
                schema: "Distance",
                table: "Distance_SportEvents");

            migrationBuilder.DropIndex(
                name: "IX_Common_Referees_CompetitionDataId",
                schema: "Common",
                table: "Common_Referees");

            migrationBuilder.DropIndex(
                name: "IX_Common_Delegations_CompetitionDataId",
                schema: "Common",
                table: "Common_Delegations");

            migrationBuilder.DropColumn(
                name: "CompetitionDataId",
                schema: "Distance",
                table: "Distance_SportEvents");

            migrationBuilder.DropColumn(
                name: "CompetitionDataId",
                schema: "Common",
                table: "Common_Referees");

            migrationBuilder.DropColumn(
                name: "CompetitionDataId",
                schema: "Common",
                table: "Common_Delegations");
        }
    }
}
