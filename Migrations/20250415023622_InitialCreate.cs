using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace worksystem.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    EmployeeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FullName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Username = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PasswordHash = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserRole = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.EmployeeId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Checkpoints",
                columns: table => new
                {
                    CheckpointId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    CheckInTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CheckOutTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    SessionStatus = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Checkpoints", x => x.CheckpointId);
                    table.ForeignKey(
                        name: "FK_Checkpoints_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Monthlyreports",
                columns: table => new
                {
                    MonthlyreportId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    ReportMonth = table.Column<DateOnly>(type: "date", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    WorkHours = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    OvertimeHours = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    MonthlyWorkDays = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    MonthlyWorkHours = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    MonthlyOvertimeHours = table.Column<decimal>(type: "decimal(65,30)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Monthlyreports", x => x.MonthlyreportId);
                    table.ForeignKey(
                        name: "FK_Monthlyreports_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Schedules",
                columns: table => new
                {
                    ScheduleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    ScheduledDate = table.Column<DateOnly>(type: "date", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time(6)", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time(6)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    ScheduledHours = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    ScheduledMonthlyHours = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    ScheduledWorkDays = table.Column<decimal>(type: "decimal(65,30)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schedules", x => x.ScheduleId);
                    table.ForeignKey(
                        name: "FK_Schedules_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Worklogs",
                columns: table => new
                {
                    WorklogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    ScheduledDate = table.Column<DateOnly>(type: "date", nullable: false),
                    WorkHours = table.Column<int>(type: "int", nullable: true),
                    OvertimeHours = table.Column<int>(type: "int", nullable: true),
                    ScheduledHours = table.Column<int>(type: "int", nullable: true),
                    ScheduledOvertime = table.Column<int>(type: "int", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    MonthlyWorkDays = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    ScheduledWorkDays = table.Column<int>(type: "int", nullable: true),
                    MonthlyOvertimeHours = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Worklogs", x => x.WorklogId);
                    table.ForeignKey(
                        name: "FK_Worklogs_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Checkpoints_EmployeeId",
                table: "Checkpoints",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Monthlyreports_EmployeeId",
                table: "Monthlyreports",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_EmployeeId",
                table: "Schedules",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Worklogs_EmployeeId",
                table: "Worklogs",
                column: "EmployeeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Checkpoints");

            migrationBuilder.DropTable(
                name: "Monthlyreports");

            migrationBuilder.DropTable(
                name: "Schedules");

            migrationBuilder.DropTable(
                name: "Worklogs");

            migrationBuilder.DropTable(
                name: "Employees");
        }
    }
}
