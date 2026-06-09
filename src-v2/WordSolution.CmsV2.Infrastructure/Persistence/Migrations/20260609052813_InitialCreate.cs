using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordSolution.CmsV2.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AtomicSections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    UpdatedTime = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AtomicSections", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Handouts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    UpdatedTime = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Handouts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OutputTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    TemplateDocxPath = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    UpdatedTime = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutputTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TeachingNotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TargetType = table.Column<int>(type: "INTEGER", nullable: false),
                    TargetId = table.Column<int>(type: "INTEGER", nullable: false),
                    NoteType = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    UpdatedTime = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeachingNotes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TeachingTopics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ParentId = table.Column<int>(type: "INTEGER", nullable: true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    UpdatedTime = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeachingTopics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeachingTopics_TeachingTopics_ParentId",
                        column: x => x.ParentId,
                        principalTable: "TeachingTopics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HandoutVersions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    HandoutId = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    UpdatedTime = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HandoutVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HandoutVersions_Handouts_HandoutId",
                        column: x => x.HandoutId,
                        principalTable: "Handouts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Sections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TeachingTopicId = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Difficulty = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    UpdatedTime = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sections_TeachingTopics_TeachingTopicId",
                        column: x => x.TeachingTopicId,
                        principalTable: "TeachingTopics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HandoutVersionItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    HandoutVersionId = table.Column<int>(type: "INTEGER", nullable: false),
                    TargetType = table.Column<int>(type: "INTEGER", nullable: false),
                    TargetId = table.Column<int>(type: "INTEGER", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    TitleOverride = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Note = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    UpdatedTime = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HandoutVersionItems", x => x.Id);
                    table.CheckConstraint("CK_HandoutVersionItems_TargetType", "\"TargetType\" IN (1, 2)");
                    table.ForeignKey(
                        name: "FK_HandoutVersionItems_HandoutVersions_HandoutVersionId",
                        column: x => x.HandoutVersionId,
                        principalTable: "HandoutVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OutputForms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    HandoutVersionId = table.Column<int>(type: "INTEGER", nullable: false),
                    OutputTemplateId = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Audience = table.Column<int>(type: "INTEGER", nullable: false),
                    OutputFormat = table.Column<int>(type: "INTEGER", nullable: false),
                    VisibilityMode = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    UpdatedTime = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutputForms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OutputForms_HandoutVersions_HandoutVersionId",
                        column: x => x.HandoutVersionId,
                        principalTable: "HandoutVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OutputForms_OutputTemplates_OutputTemplateId",
                        column: x => x.OutputTemplateId,
                        principalTable: "OutputTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SectionVariants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SectionId = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Difficulty = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    UpdatedTime = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SectionVariants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SectionVariants_Sections_SectionId",
                        column: x => x.SectionId,
                        principalTable: "Sections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GeneratedFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OutputFormId = table.Column<int>(type: "INTEGER", nullable: false),
                    FilePath = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: false),
                    VersionManifestJson = table.Column<string>(type: "TEXT", nullable: false),
                    GeneratedTime = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneratedFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GeneratedFiles_OutputForms_OutputFormId",
                        column: x => x.OutputFormId,
                        principalTable: "OutputForms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AtomicSectionItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AtomicSectionId = table.Column<int>(type: "INTEGER", nullable: false),
                    ContentBlockId = table.Column<int>(type: "INTEGER", nullable: false),
                    ReferenceMode = table.Column<int>(type: "INTEGER", nullable: false),
                    LockedContentBlockVersionId = table.Column<int>(type: "INTEGER", nullable: true),
                    TitleOverride = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    Note = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    UpdatedTime = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AtomicSectionItems", x => x.Id);
                    table.CheckConstraint("CK_AtomicSectionItems_LockedVersionRequiresVersion", "\"ReferenceMode\" <> 2 OR \"LockedContentBlockVersionId\" IS NOT NULL");
                    table.ForeignKey(
                        name: "FK_AtomicSectionItems_AtomicSections_AtomicSectionId",
                        column: x => x.AtomicSectionId,
                        principalTable: "AtomicSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ContentBlockRelations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ParentBlockId = table.Column<int>(type: "INTEGER", nullable: false),
                    ChildBlockId = table.Column<int>(type: "INTEGER", nullable: false),
                    ReferenceMode = table.Column<int>(type: "INTEGER", nullable: false),
                    LockedContentBlockVersionId = table.Column<int>(type: "INTEGER", nullable: true),
                    TitleOverride = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    Note = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    UpdatedTime = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentBlockRelations", x => x.Id);
                    table.CheckConstraint("CK_ContentBlockRelations_LockedVersionRequiresVersion", "\"ReferenceMode\" <> 2 OR \"LockedContentBlockVersionId\" IS NOT NULL");
                    table.CheckConstraint("CK_ContentBlockRelations_NoDirectSelfReference", "\"ParentBlockId\" <> \"ChildBlockId\"");
                });

            migrationBuilder.CreateTable(
                name: "ContentBlocks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Summary = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    BlockType = table.Column<int>(type: "INTEGER", nullable: false),
                    Difficulty = table.Column<int>(type: "INTEGER", nullable: false),
                    QuestionType = table.Column<int>(type: "INTEGER", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CurrentVersionId = table.Column<int>(type: "INTEGER", nullable: true),
                    UpdatedTime = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentBlocks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContentBlockVersions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ContentBlockId = table.Column<int>(type: "INTEGER", nullable: false),
                    VersionNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    DocxPath = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: false),
                    HtmlPreviewPath = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: true),
                    PlainText = table.Column<string>(type: "TEXT", nullable: true),
                    IsCurrent = table.Column<bool>(type: "INTEGER", nullable: false),
                    UpdatedTime = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentBlockVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContentBlockVersions_ContentBlocks_ContentBlockId",
                        column: x => x.ContentBlockId,
                        principalTable: "ContentBlocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SectionItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SectionId = table.Column<int>(type: "INTEGER", nullable: false),
                    TargetType = table.Column<int>(type: "INTEGER", nullable: false),
                    TargetId = table.Column<int>(type: "INTEGER", nullable: false),
                    ReferenceMode = table.Column<int>(type: "INTEGER", nullable: false),
                    LockedContentBlockVersionId = table.Column<int>(type: "INTEGER", nullable: true),
                    TitleOverride = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    ParentItemId = table.Column<int>(type: "INTEGER", nullable: true),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    SelectionLayer = table.Column<int>(type: "INTEGER", nullable: true),
                    TeachingUseOverride = table.Column<int>(type: "INTEGER", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Note = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    UpdatedTime = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SectionItems", x => x.Id);
                    table.CheckConstraint("CK_SectionItems_ContentBlockReferenceMode", "\"TargetType\" = 1 OR (\"ReferenceMode\" = 1 AND \"LockedContentBlockVersionId\" IS NULL)");
                    table.CheckConstraint("CK_SectionItems_LockedVersionRequiresVersion", "\"ReferenceMode\" <> 2 OR \"LockedContentBlockVersionId\" IS NOT NULL");
                    table.CheckConstraint("CK_SectionItems_TargetType", "\"TargetType\" IN (1, 2)");
                    table.ForeignKey(
                        name: "FK_SectionItems_ContentBlockVersions_LockedContentBlockVersionId",
                        column: x => x.LockedContentBlockVersionId,
                        principalTable: "ContentBlockVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SectionItems_SectionItems_ParentItemId",
                        column: x => x.ParentItemId,
                        principalTable: "SectionItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SectionItems_Sections_SectionId",
                        column: x => x.SectionId,
                        principalTable: "Sections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SectionVariantItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SectionVariantId = table.Column<int>(type: "INTEGER", nullable: false),
                    SectionItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    Note = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    UpdatedTime = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SectionVariantItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SectionVariantItems_SectionItems_SectionItemId",
                        column: x => x.SectionItemId,
                        principalTable: "SectionItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SectionVariantItems_SectionVariants_SectionVariantId",
                        column: x => x.SectionVariantId,
                        principalTable: "SectionVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AtomicSectionItems_AtomicSectionId_SortOrder",
                table: "AtomicSectionItems",
                columns: new[] { "AtomicSectionId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_AtomicSectionItems_ContentBlockId",
                table: "AtomicSectionItems",
                column: "ContentBlockId");

            migrationBuilder.CreateIndex(
                name: "IX_AtomicSectionItems_LockedContentBlockVersionId",
                table: "AtomicSectionItems",
                column: "LockedContentBlockVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentBlockRelations_ChildBlockId",
                table: "ContentBlockRelations",
                column: "ChildBlockId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentBlockRelations_LockedContentBlockVersionId",
                table: "ContentBlockRelations",
                column: "LockedContentBlockVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentBlockRelations_ParentBlockId_SortOrder",
                table: "ContentBlockRelations",
                columns: new[] { "ParentBlockId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_ContentBlocks_CurrentVersionId",
                table: "ContentBlocks",
                column: "CurrentVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentBlockVersions_ContentBlockId_IsCurrent",
                table: "ContentBlockVersions",
                columns: new[] { "ContentBlockId", "IsCurrent" });

            migrationBuilder.CreateIndex(
                name: "IX_ContentBlockVersions_ContentBlockId_VersionNumber",
                table: "ContentBlockVersions",
                columns: new[] { "ContentBlockId", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GeneratedFiles_OutputFormId",
                table: "GeneratedFiles",
                column: "OutputFormId");

            migrationBuilder.CreateIndex(
                name: "IX_HandoutVersionItems_HandoutVersionId_SortOrder",
                table: "HandoutVersionItems",
                columns: new[] { "HandoutVersionId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_HandoutVersionItems_TargetType_TargetId",
                table: "HandoutVersionItems",
                columns: new[] { "TargetType", "TargetId" });

            migrationBuilder.CreateIndex(
                name: "IX_HandoutVersions_HandoutId_SortOrder",
                table: "HandoutVersions",
                columns: new[] { "HandoutId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_OutputForms_HandoutVersionId_SortOrder",
                table: "OutputForms",
                columns: new[] { "HandoutVersionId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_OutputForms_OutputTemplateId",
                table: "OutputForms",
                column: "OutputTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_SectionItems_LockedContentBlockVersionId",
                table: "SectionItems",
                column: "LockedContentBlockVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_SectionItems_ParentItemId",
                table: "SectionItems",
                column: "ParentItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SectionItems_SectionId_SortOrder",
                table: "SectionItems",
                columns: new[] { "SectionId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_SectionItems_TargetType_TargetId",
                table: "SectionItems",
                columns: new[] { "TargetType", "TargetId" });

            migrationBuilder.CreateIndex(
                name: "IX_Sections_TeachingTopicId_SortOrder",
                table: "Sections",
                columns: new[] { "TeachingTopicId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_SectionVariantItems_SectionItemId",
                table: "SectionVariantItems",
                column: "SectionItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SectionVariantItems_SectionVariantId_SectionItemId",
                table: "SectionVariantItems",
                columns: new[] { "SectionVariantId", "SectionItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SectionVariantItems_SectionVariantId_SortOrder",
                table: "SectionVariantItems",
                columns: new[] { "SectionVariantId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_SectionVariants_SectionId_SortOrder",
                table: "SectionVariants",
                columns: new[] { "SectionId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_TeachingNotes_TargetType_TargetId",
                table: "TeachingNotes",
                columns: new[] { "TargetType", "TargetId" });

            migrationBuilder.CreateIndex(
                name: "IX_TeachingTopics_ParentId",
                table: "TeachingTopics",
                column: "ParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_AtomicSectionItems_ContentBlockVersions_LockedContentBlockVersionId",
                table: "AtomicSectionItems",
                column: "LockedContentBlockVersionId",
                principalTable: "ContentBlockVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AtomicSectionItems_ContentBlocks_ContentBlockId",
                table: "AtomicSectionItems",
                column: "ContentBlockId",
                principalTable: "ContentBlocks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ContentBlockRelations_ContentBlockVersions_LockedContentBlockVersionId",
                table: "ContentBlockRelations",
                column: "LockedContentBlockVersionId",
                principalTable: "ContentBlockVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ContentBlockRelations_ContentBlocks_ChildBlockId",
                table: "ContentBlockRelations",
                column: "ChildBlockId",
                principalTable: "ContentBlocks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ContentBlockRelations_ContentBlocks_ParentBlockId",
                table: "ContentBlockRelations",
                column: "ParentBlockId",
                principalTable: "ContentBlocks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ContentBlocks_ContentBlockVersions_CurrentVersionId",
                table: "ContentBlocks",
                column: "CurrentVersionId",
                principalTable: "ContentBlockVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContentBlocks_ContentBlockVersions_CurrentVersionId",
                table: "ContentBlocks");

            migrationBuilder.DropTable(
                name: "AtomicSectionItems");

            migrationBuilder.DropTable(
                name: "ContentBlockRelations");

            migrationBuilder.DropTable(
                name: "GeneratedFiles");

            migrationBuilder.DropTable(
                name: "HandoutVersionItems");

            migrationBuilder.DropTable(
                name: "SectionVariantItems");

            migrationBuilder.DropTable(
                name: "TeachingNotes");

            migrationBuilder.DropTable(
                name: "AtomicSections");

            migrationBuilder.DropTable(
                name: "OutputForms");

            migrationBuilder.DropTable(
                name: "SectionItems");

            migrationBuilder.DropTable(
                name: "SectionVariants");

            migrationBuilder.DropTable(
                name: "HandoutVersions");

            migrationBuilder.DropTable(
                name: "OutputTemplates");

            migrationBuilder.DropTable(
                name: "Sections");

            migrationBuilder.DropTable(
                name: "Handouts");

            migrationBuilder.DropTable(
                name: "TeachingTopics");

            migrationBuilder.DropTable(
                name: "ContentBlockVersions");

            migrationBuilder.DropTable(
                name: "ContentBlocks");
        }
    }
}
