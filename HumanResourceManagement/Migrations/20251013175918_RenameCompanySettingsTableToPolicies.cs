using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HumanResourceManagement.Migrations
{
    /// <inheritdoc />
    public partial class RenameCompanySettingsTableToPolicies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_CompanySettings",
                table: "CompanySettings");

            migrationBuilder.RenameTable(
                name: "CompanySettings",
                newName: "Policies");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Policies",
                table: "Policies",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Policies",
                table: "Policies");

            migrationBuilder.RenameTable(
                name: "Policies",
                newName: "CompanySettings");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CompanySettings",
                table: "CompanySettings",
                column: "Id");
        }
    }
}
