using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.DbContexts.Migrations
{
    /// <inheritdoc />
    public partial class AddShortNameAndUpdateConductingOrganizationsToCompetitionData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ConductingOrganizations",
                schema: "Common",
                table: "Common_CompetitionData",
                type: "json",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 500);

            migrationBuilder.AddColumn<string>(
                name: "ShortName",
                schema: "Common",
                table: "Common_CompetitionData",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShortName",
                schema: "Common",
                table: "Common_CompetitionData");

            migrationBuilder.AlterColumn<string>(
                name: "ConductingOrganizations",
                schema: "Common",
                table: "Common_CompetitionData",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "json");
        }
    }
}
