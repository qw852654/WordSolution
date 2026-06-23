using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordSolution.CmsV2.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAtomicSectionPanels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AtomicSectionPanelId",
                table: "AtomicSectionItems",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TeachingRole",
                table: "AtomicSectionItems",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "AtomicSectionPanels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AtomicSectionId = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    TeachingRole = table.Column<int>(type: "INTEGER", nullable: false),
                    Difficulty = table.Column<int>(type: "INTEGER", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    UpdatedTime = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AtomicSectionPanels", x => x.Id);
                    table.CheckConstraint("CK_AtomicSectionPanels_TeachingRole", "\"TeachingRole\" <> 0");
                    table.ForeignKey(
                        name: "FK_AtomicSectionPanels_AtomicSections_AtomicSectionId",
                        column: x => x.AtomicSectionId,
                        principalTable: "AtomicSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AtomicSectionItems_AtomicSectionId_AtomicSectionPanelId_SortOrder",
                table: "AtomicSectionItems",
                columns: new[] { "AtomicSectionId", "AtomicSectionPanelId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_AtomicSectionItems_AtomicSectionPanelId",
                table: "AtomicSectionItems",
                column: "AtomicSectionPanelId");

            migrationBuilder.CreateIndex(
                name: "IX_AtomicSectionPanels_AtomicSectionId_SortOrder",
                table: "AtomicSectionPanels",
                columns: new[] { "AtomicSectionId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "UX_AtomicSectionPanels_AtomicSectionId_TeachingRole_Difficulty",
                table: "AtomicSectionPanels",
                columns: new[] { "AtomicSectionId", "TeachingRole", "Difficulty" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AtomicSectionItems_AtomicSectionPanels_AtomicSectionPanelId",
                table: "AtomicSectionItems",
                column: "AtomicSectionPanelId",
                principalTable: "AtomicSectionPanels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AtomicSectionItems_AtomicSectionPanels_AtomicSectionPanelId",
                table: "AtomicSectionItems");

            migrationBuilder.DropTable(
                name: "AtomicSectionPanels");

            migrationBuilder.DropIndex(
                name: "IX_AtomicSectionItems_AtomicSectionId_AtomicSectionPanelId_SortOrder",
                table: "AtomicSectionItems");

            migrationBuilder.DropIndex(
                name: "IX_AtomicSectionItems_AtomicSectionPanelId",
                table: "AtomicSectionItems");

            migrationBuilder.DropColumn(
                name: "AtomicSectionPanelId",
                table: "AtomicSectionItems");

            migrationBuilder.DropColumn(
                name: "TeachingRole",
                table: "AtomicSectionItems");
        }
    }
}
