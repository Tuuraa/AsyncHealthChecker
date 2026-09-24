using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AsyncHealthChecker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedCheckTasks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CheckTasks",
                columns: table => new
                {
                    TaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    UrlsCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CheckTasks", x => x.TaskId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CheckTasks");
        }
    }
}
