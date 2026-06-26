using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordSolution.CmsV2.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPreClassQuizPanels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                INSERT INTO AtomicSectionPanels (AtomicSectionId, Title, TeachingRole, Difficulty, SortOrder, UpdatedTime)
                SELECT
                    atomicSection.Id,
                    atomicSection.Title,
                    6,
                    atomicSection.Difficulty,
                    COALESCE(
                        (
                            SELECT MAX(existingPanel.SortOrder) + 10
                            FROM AtomicSectionPanels AS existingPanel
                            WHERE existingPanel.AtomicSectionId = atomicSection.Id
                        ),
                        40
                    ),
                    strftime('%Y-%m-%dT%H:%M:%f+00:00', 'now')
                FROM AtomicSections AS atomicSection
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM AtomicSectionPanels AS preClassQuizPanel
                    WHERE preClassQuizPanel.AtomicSectionId = atomicSection.Id
                      AND preClassQuizPanel.TeachingRole = 6
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Data backfill is retained on rollback to avoid deleting existing or already-used panels.
        }
    }
}
