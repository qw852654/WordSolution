using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordSolution.CmsV2.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddContentBlockVersionParts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PartParseMessage",
                table: "ContentBlockVersions",
                type: "TEXT",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PartParseStatus",
                table: "ContentBlockVersions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ContentBlockVersionParts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ContentBlockVersionId = table.Column<int>(type: "INTEGER", nullable: false),
                    PartType = table.Column<int>(type: "INTEGER", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    PlainText = table.Column<string>(type: "TEXT", nullable: true),
                    SourceStyleNamesJson = table.Column<string>(type: "TEXT", nullable: false),
                    WarningMessage = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentBlockVersionParts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContentBlockVersionParts_ContentBlockVersions_ContentBlockVersionId",
                        column: x => x.ContentBlockVersionId,
                        principalTable: "ContentBlockVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContentBlockVersionParts_ContentBlockVersionId_PartType",
                table: "ContentBlockVersionParts",
                columns: new[] { "ContentBlockVersionId", "PartType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContentBlockVersionParts_ContentBlockVersionId_SortOrder",
                table: "ContentBlockVersionParts",
                columns: new[] { "ContentBlockVersionId", "SortOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContentBlockVersionParts");

            migrationBuilder.DropColumn(
                name: "PartParseMessage",
                table: "ContentBlockVersions");

            migrationBuilder.DropColumn(
                name: "PartParseStatus",
                table: "ContentBlockVersions");
        }
    }
}
