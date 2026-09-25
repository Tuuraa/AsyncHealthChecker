using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AsyncHealthChecker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedIndexToCheckResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_CheckResults_TaskId_Url",
                table: "CheckResults",
                columns: new[] { "TaskId", "Url" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CheckResults_TaskId_Url",
                table: "CheckResults");
        }
    }
}
