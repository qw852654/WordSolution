using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordSolution.CmsV2.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RebuildTeachingNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TeachingNotes_TargetType_TargetId",
                table: "TeachingNotes");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "TeachingNotes");

            migrationBuilder.DropColumn(
                name: "TargetId",
                table: "TeachingNotes");

            migrationBuilder.DropColumn(
                name: "TargetType",
                table: "TeachingNotes");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "TeachingNotes");

            migrationBuilder.AddColumn<string>(
                name: "CreatedTime",
                table: "TeachingNotes",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "EffectLevel",
                table: "TeachingNotes",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "OccurredAt",
                table: "TeachingNotes",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TeachingNoteBindings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TeachingNoteId = table.Column<int>(type: "INTEGER", nullable: false),
                    TargetType = table.Column<int>(type: "INTEGER", nullable: false),
                    TargetId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedTime = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeachingNoteBindings", x => x.Id);
                    table.CheckConstraint("CK_TeachingNoteBindings_TargetId", "\"TargetId\" > 0");
                    table.CheckConstraint("CK_TeachingNoteBindings_TargetType", "\"TargetType\" IN (1, 2, 3, 4, 5, 6)");
                    table.ForeignKey(
                        name: "FK_TeachingNoteBindings_TeachingNotes_TeachingNoteId",
                        column: x => x.TeachingNoteId,
                        principalTable: "TeachingNotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TeachingNotes_UpdatedTime",
                table: "TeachingNotes",
                column: "UpdatedTime");

            migrationBuilder.AddCheckConstraint(
                name: "CK_TeachingNotes_EffectLevel",
                table: "TeachingNotes",
                sql: "\"EffectLevel\" IS NULL OR \"EffectLevel\" IN (0, 1, 2, 3, 4)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_TeachingNotes_NoteType",
                table: "TeachingNotes",
                sql: "\"NoteType\" IN (1, 2, 3, 4, 5, 6, 7)");

            migrationBuilder.CreateIndex(
                name: "IX_TeachingNoteBindings_TargetType_TargetId",
                table: "TeachingNoteBindings",
                columns: new[] { "TargetType", "TargetId" });

            migrationBuilder.CreateIndex(
                name: "IX_TeachingNoteBindings_TeachingNoteId_TargetType_TargetId",
                table: "TeachingNoteBindings",
                columns: new[] { "TeachingNoteId", "TargetType", "TargetId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TeachingNoteBindings");

            migrationBuilder.DropIndex(
                name: "IX_TeachingNotes_UpdatedTime",
                table: "TeachingNotes");

            migrationBuilder.DropCheckConstraint(
                name: "CK_TeachingNotes_EffectLevel",
                table: "TeachingNotes");

            migrationBuilder.DropCheckConstraint(
                name: "CK_TeachingNotes_NoteType",
                table: "TeachingNotes");

            migrationBuilder.DropColumn(
                name: "CreatedTime",
                table: "TeachingNotes");

            migrationBuilder.DropColumn(
                name: "EffectLevel",
                table: "TeachingNotes");

            migrationBuilder.DropColumn(
                name: "OccurredAt",
                table: "TeachingNotes");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "TeachingNotes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TargetId",
                table: "TeachingNotes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TargetType",
                table: "TeachingNotes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "TeachingNotes",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_TeachingNotes_TargetType_TargetId",
                table: "TeachingNotes",
                columns: new[] { "TargetType", "TargetId" });
        }
    }
}
