using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HumanResourceManagement.Migrations
{
    /// <inheritdoc />
    public partial class ReplacePolicyWithCompanySettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Policies");

            migrationBuilder.CreateTable(
                name: "CompanySettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkStartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    WorkEndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    LateEarlyThresholdMinutes = table.Column<int>(type: "int", nullable: false),
                    LateEarlyDeductionPercent = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    MonthlyOvertimeHoursLimit = table.Column<int>(type: "int", nullable: false),
                    AnnualLeaveMaxDays = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanySettings", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "CompanySettings",
                columns: new[] { "Id", "AnnualLeaveMaxDays", "LateEarlyDeductionPercent", "LateEarlyThresholdMinutes", "MonthlyOvertimeHoursLimit", "WorkEndTime", "WorkStartTime" },
                values: new object[] { 1, 12, 10m, 15, 40, new TimeSpan(0, 17, 0, 0, 0), new TimeSpan(0, 9, 0, 0, 0) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompanySettings");

            migrationBuilder.CreateTable(
                name: "Policies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Policies", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Policy_Name_Unique",
                table: "Policies",
                column: "Name",
                unique: true);
        }
    }
}
