using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.DbContexts.Migrations
{
    /// <inheritdoc />
    public partial class _1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Referees_RefereeingPositionId",
                schema: "Common",
                table: "Common_Referees");

            migrationBuilder.DropIndex(
                name: "IX_Common_Referees_RefereeJobTitleId",
                schema: "Common",
                table: "Common_Referees");

            migrationBuilder.CreateIndex(
                name: "IX_Common_Referees_RefereeJobTitleId",
                schema: "Common",
                table: "Common_Referees",
                column: "RefereeJobTitleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Referees_RefereeJobTitleId",
                schema: "Common",
                table: "Common_Referees",
                column: "RefereeJobTitleId",
                principalSchema: "Library",
                principalTable: "Lib_RefereeJobTitles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Referees_RefereeJobTitleId",
                schema: "Common",
                table: "Common_Referees");

            migrationBuilder.DropIndex(
                name: "IX_Common_Referees_RefereeJobTitleId",
                schema: "Common",
                table: "Common_Referees");

            migrationBuilder.CreateIndex(
                name: "IX_Common_Referees_RefereeJobTitleId",
                schema: "Common",
                table: "Common_Referees",
                column: "RefereeJobTitleId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Referees_RefereeingPositionId",
                schema: "Common",
                table: "Common_Referees",
                column: "RefereeJobTitleId",
                principalSchema: "Library",
                principalTable: "Lib_RefereeJobTitles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
