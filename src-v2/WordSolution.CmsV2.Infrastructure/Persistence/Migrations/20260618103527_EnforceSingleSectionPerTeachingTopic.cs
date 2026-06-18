using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordSolution.CmsV2.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EnforceSingleSectionPerTeachingTopic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Sections_TeachingTopicId",
                table: "Sections",
                column: "TeachingTopicId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sections_TeachingTopicId",
                table: "Sections");
        }
    }
}
