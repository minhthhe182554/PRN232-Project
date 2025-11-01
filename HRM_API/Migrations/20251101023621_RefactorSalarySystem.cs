using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HRM_API.Migrations
{
    /// <inheritdoc />
    public partial class RefactorSalarySystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Salary",
                table: "Users");

            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "SalaryScales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Role = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    BaseSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalaryScales", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "SalaryScales",
                columns: new[] { "Id", "BaseSalary", "Description", "Level", "Role" },
                values: new object[,]
                {
                    { 1, 50000000m, "Admin Level 1", 1, "Admin" },
                    { 2, 30000000m, "Manager Level 1", 1, "Manager" },
                    { 3, 40000000m, "Manager Level 2", 2, "Manager" },
                    { 4, 15000000m, "Employee Level 1", 1, "Employee" },
                    { 5, 20000000m, "Employee Level 2", 2, "Employee" },
                    { 6, 25000000m, "Employee Level 3", 3, "Employee" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalaryScales_Role_Level",
                table: "SalaryScales",
                columns: new[] { "Role", "Level" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SalaryScales");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "Users");

            migrationBuilder.AddColumn<decimal>(
                name: "Salary",
                table: "Users",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
