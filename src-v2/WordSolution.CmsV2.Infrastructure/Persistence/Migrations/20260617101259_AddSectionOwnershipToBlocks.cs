using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordSolution.CmsV2.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSectionOwnershipToBlocks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SectionId",
                table: "ContentBlocks",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Difficulty",
                table: "AtomicSections",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SectionId",
                table: "AtomicSections",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(
                """
                UPDATE ContentBlocks
                SET SectionId = (SELECT Id FROM Sections ORDER BY Id LIMIT 1)
                WHERE SectionId = 0 AND EXISTS (SELECT 1 FROM Sections);
                """);

            migrationBuilder.Sql(
                """
                UPDATE AtomicSections
                SET SectionId = (SELECT Id FROM Sections ORDER BY Id LIMIT 1)
                WHERE SectionId = 0 AND EXISTS (SELECT 1 FROM Sections);
                """);

            migrationBuilder.CreateIndex(
                name: "IX_ContentBlocks_SectionId",
                table: "ContentBlocks",
                column: "SectionId");

            migrationBuilder.CreateIndex(
                name: "IX_AtomicSections_SectionId",
                table: "AtomicSections",
                column: "SectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_AtomicSections_Sections_SectionId",
                table: "AtomicSections",
                column: "SectionId",
                principalTable: "Sections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ContentBlocks_Sections_SectionId",
                table: "ContentBlocks",
                column: "SectionId",
                principalTable: "Sections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AtomicSections_Sections_SectionId",
                table: "AtomicSections");

            migrationBuilder.DropForeignKey(
                name: "FK_ContentBlocks_Sections_SectionId",
                table: "ContentBlocks");

            migrationBuilder.DropIndex(
                name: "IX_ContentBlocks_SectionId",
                table: "ContentBlocks");

            migrationBuilder.DropIndex(
                name: "IX_AtomicSections_SectionId",
                table: "AtomicSections");

            migrationBuilder.DropColumn(
                name: "SectionId",
                table: "ContentBlocks");

            migrationBuilder.DropColumn(
                name: "Difficulty",
                table: "AtomicSections");

            migrationBuilder.DropColumn(
                name: "SectionId",
                table: "AtomicSections");
        }
    }
}
