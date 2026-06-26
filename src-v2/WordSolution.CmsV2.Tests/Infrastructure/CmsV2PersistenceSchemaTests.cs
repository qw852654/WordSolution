using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using WordSolution.CmsV2.Infrastructure.Persistence;

namespace WordSolution.CmsV2.Tests.Infrastructure;

public sealed class CmsV2PersistenceSchemaTests
{
    private static readonly string[] ExpectedTableNames =
    [
        "TeachingTopics",
        "Sections",
        "SectionItems",
        "AtomicSections",
        "AtomicSectionPanels",
        "AtomicSectionItems",
        "SectionVariants",
        "SectionVariantItems",
        "ContentBlocks",
        "ContentBlockVersions",
        "ContentBlockVersionParts",
        "ContentBlockRelations",
        "Handouts",
        "HandoutVersions",
        "HandoutVersionItems",
        "OutputTemplates",
        "OutputForms",
        "GeneratedFiles",
        "Tags",
        "TagBindings",
        "TeachingNotes",
        "TeachingNoteBindings"
    ];

    [Fact]
    public void DatabasePath_uses_v2_database_file_without_legacy_database_name()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

        var path = CmsV2DatabasePaths.GetDatabasePath(root);

        Assert.Equal(Path.Combine(root, "cms-v2.db"), path);
        Assert.False(path.Contains("question-bank.db", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Migrations_create_all_v2_tables_in_new_sqlite_database()
    {
        var root = CreateTempDirectory();
        var databasePath = CmsV2DatabasePaths.GetDatabasePath(root);

        await using var context = CmsV2DbContextFactory.CreateForDatabase(databasePath);
        await context.Database.MigrateAsync();

        var tables = await ReadScalarStringsAsync(
            context,
            "SELECT name FROM sqlite_master WHERE type = 'table' AND name NOT LIKE 'sqlite_%' AND name NOT LIKE '__EF%';");

        foreach (var tableName in ExpectedTableNames)
        {
            Assert.Contains(tableName, tables);
        }

        Assert.Equal(ExpectedTableNames.Length, tables.Count);
        Assert.True(File.Exists(databasePath));
        Assert.False(File.Exists(Path.Combine(root, "question-bank.db")));
    }

    [Fact]
    public async Task Schema_uses_expected_columns_without_created_time_or_removed_content_version_fields()
    {
        var databasePath = Path.Combine(CreateTempDirectory(), "cms-v2.db");

        await using var context = CmsV2DbContextFactory.CreateForDatabase(databasePath);
        await context.Database.MigrateAsync();

        foreach (var tableName in ExpectedTableNames)
        {
            var columns = await ReadTableColumnsAsync(context, tableName);
            Assert.Contains("Id", columns);
            if (tableName is not "Tags" and not "TagBindings" and not "TeachingNotes" and not "TeachingNoteBindings")
            {
                Assert.DoesNotContain("CreatedTime", columns);
            }
        }

        var generatedFileColumns = await ReadTableColumnsAsync(context, "GeneratedFiles");
        Assert.Contains("GeneratedTime", generatedFileColumns);

        var contentBlockVersionColumns = await ReadTableColumnsAsync(context, "ContentBlockVersions");
        Assert.DoesNotContain("Difficulty", contentBlockVersionColumns);
        Assert.DoesNotContain("BlockType", contentBlockVersionColumns);
        Assert.DoesNotContain("QuestionType", contentBlockVersionColumns);
        Assert.Contains("PartParseStatus", contentBlockVersionColumns);
        Assert.Contains("PartParseMessage", contentBlockVersionColumns);

        var contentBlockVersionPartColumns = await ReadTableColumnsAsync(context, "ContentBlockVersionParts");
        Assert.Contains("ContentBlockVersionId", contentBlockVersionPartColumns);
        Assert.Contains("PartType", contentBlockVersionPartColumns);
        Assert.Contains("SortOrder", contentBlockVersionPartColumns);
        Assert.Contains("PlainText", contentBlockVersionPartColumns);
        Assert.Contains("SourceStyleNamesJson", contentBlockVersionPartColumns);
        Assert.Contains("WarningMessage", contentBlockVersionPartColumns);

        var atomicSectionPanelColumns = await ReadTableColumnsAsync(context, "AtomicSectionPanels");
        Assert.Contains("AtomicSectionId", atomicSectionPanelColumns);
        Assert.Contains("Title", atomicSectionPanelColumns);
        Assert.Contains("TeachingRole", atomicSectionPanelColumns);
        Assert.Contains("Difficulty", atomicSectionPanelColumns);
        Assert.Contains("SortOrder", atomicSectionPanelColumns);

        var atomicSectionItemColumns = await ReadTableColumnsAsync(context, "AtomicSectionItems");
        Assert.Contains("AtomicSectionPanelId", atomicSectionItemColumns);
        Assert.Contains("TeachingRole", atomicSectionItemColumns);

        var tagColumns = await ReadTableColumnsAsync(context, "Tags");
        Assert.Contains("Name", tagColumns);
        Assert.Contains("NormalizedName", tagColumns);
        Assert.Contains("Color", tagColumns);
        Assert.Contains("Status", tagColumns);
        Assert.Contains("CreatedTime", tagColumns);
        Assert.Contains("UpdatedTime", tagColumns);

        var tagBindingColumns = await ReadTableColumnsAsync(context, "TagBindings");
        Assert.Contains("TagId", tagBindingColumns);
        Assert.Contains("TargetType", tagBindingColumns);
        Assert.Contains("TargetId", tagBindingColumns);
        Assert.Contains("CreatedTime", tagBindingColumns);
        Assert.Contains("UpdatedTime", tagBindingColumns);

        var teachingNoteColumns = await ReadTableColumnsAsync(context, "TeachingNotes");
        Assert.Contains("NoteType", teachingNoteColumns);
        Assert.Contains("Content", teachingNoteColumns);
        Assert.Contains("EffectLevel", teachingNoteColumns);
        Assert.Contains("OccurredAt", teachingNoteColumns);
        Assert.Contains("CreatedTime", teachingNoteColumns);
        Assert.Contains("UpdatedTime", teachingNoteColumns);
        Assert.DoesNotContain("Title", teachingNoteColumns);
        Assert.DoesNotContain("Status", teachingNoteColumns);
        Assert.DoesNotContain("NextAction", teachingNoteColumns);
        Assert.DoesNotContain("SortOrder", teachingNoteColumns);
        Assert.DoesNotContain("TargetType", teachingNoteColumns);
        Assert.DoesNotContain("TargetId", teachingNoteColumns);

        var teachingNoteBindingColumns = await ReadTableColumnsAsync(context, "TeachingNoteBindings");
        Assert.Contains("TeachingNoteId", teachingNoteBindingColumns);
        Assert.Contains("TargetType", teachingNoteBindingColumns);
        Assert.Contains("TargetId", teachingNoteBindingColumns);
        Assert.Contains("CreatedTime", teachingNoteBindingColumns);
        Assert.DoesNotContain("UpdatedTime", teachingNoteBindingColumns);
    }

    [Fact]
    public async Task Schema_has_expected_indexes_and_restrict_foreign_keys()
    {
        var databasePath = Path.Combine(CreateTempDirectory(), "cms-v2.db");

        await using var context = CmsV2DbContextFactory.CreateForDatabase(databasePath);
        await context.Database.MigrateAsync();

        var contentVersionIndexes = await ReadIndexesAsync(context, "ContentBlockVersions");
        Assert.Contains(
            contentVersionIndexes,
            index => index.IsUnique && index.Columns.SequenceEqual(["ContentBlockId", "VersionNumber"]));

        var contentVersionPartIndexes = await ReadIndexesAsync(context, "ContentBlockVersionParts");
        Assert.Contains(
            contentVersionPartIndexes,
            index => index.IsUnique && index.Columns.SequenceEqual(["ContentBlockVersionId", "PartType"]));
        Assert.Contains(
            contentVersionPartIndexes,
            index => index.Columns.SequenceEqual(["ContentBlockVersionId", "SortOrder"]));

        var contentVersionPartForeignKeys = await ReadForeignKeysAsync(context, "ContentBlockVersionParts");
        Assert.Contains(
            contentVersionPartForeignKeys,
            foreignKey => foreignKey.Table == "ContentBlockVersions"
                && foreignKey.From == "ContentBlockVersionId"
                && foreignKey.OnDelete == "RESTRICT");

        var sectionIndexes = await ReadIndexesAsync(context, "Sections");
        Assert.Contains(
            sectionIndexes,
            index => index.IsUnique && index.Columns.SequenceEqual(["TeachingTopicId"]));

        var atomicSectionPanelIndexes = await ReadIndexesAsync(context, "AtomicSectionPanels");
        Assert.Contains(
            atomicSectionPanelIndexes,
            index => index.Columns.SequenceEqual(["AtomicSectionId", "SortOrder"]));
        Assert.Contains(
            atomicSectionPanelIndexes,
            index => index.IsUnique
                && index.Columns.SequenceEqual(["AtomicSectionId", "TeachingRole", "Difficulty"]));

        var atomicSectionItemIndexes = await ReadIndexesAsync(context, "AtomicSectionItems");
        Assert.Contains(
            atomicSectionItemIndexes,
            index => index.Columns.SequenceEqual(["AtomicSectionId", "AtomicSectionPanelId", "SortOrder"]));

        var atomicSectionPanelForeignKeys = await ReadForeignKeysAsync(context, "AtomicSectionPanels");
        Assert.Contains(
            atomicSectionPanelForeignKeys,
            foreignKey => foreignKey.Table == "AtomicSections"
                && foreignKey.From == "AtomicSectionId"
                && foreignKey.OnDelete == "RESTRICT");

        var atomicSectionItemForeignKeys = await ReadForeignKeysAsync(context, "AtomicSectionItems");
        Assert.Contains(
            atomicSectionItemForeignKeys,
            foreignKey => foreignKey.Table == "AtomicSectionPanels"
                && foreignKey.From == "AtomicSectionPanelId"
                && foreignKey.OnDelete == "RESTRICT");

        var tagIndexes = await ReadIndexesAsync(context, "Tags");
        Assert.Contains(
            tagIndexes,
            index => index.IsUnique && index.Columns.SequenceEqual(["NormalizedName"]));

        var tagBindingIndexes = await ReadIndexesAsync(context, "TagBindings");
        Assert.Contains(
            tagBindingIndexes,
            index => index.IsUnique && index.Columns.SequenceEqual(["TagId", "TargetType", "TargetId"]));
        Assert.Contains(
            tagBindingIndexes,
            index => index.Columns.SequenceEqual(["TargetType", "TargetId"]));

        var tagBindingForeignKeys = await ReadForeignKeysAsync(context, "TagBindings");
        Assert.Contains(
            tagBindingForeignKeys,
            foreignKey => foreignKey.Table == "Tags"
                && foreignKey.From == "TagId"
                && foreignKey.OnDelete == "RESTRICT");

        var teachingNoteIndexes = await ReadIndexesAsync(context, "TeachingNotes");
        Assert.Contains(
            teachingNoteIndexes,
            index => index.Columns.SequenceEqual(["UpdatedTime"]));

        var teachingNoteBindingIndexes = await ReadIndexesAsync(context, "TeachingNoteBindings");
        Assert.Contains(
            teachingNoteBindingIndexes,
            index => index.IsUnique && index.Columns.SequenceEqual(["TeachingNoteId", "TargetType", "TargetId"]));
        Assert.Contains(
            teachingNoteBindingIndexes,
            index => index.Columns.SequenceEqual(["TargetType", "TargetId"]));

        var teachingNoteBindingForeignKeys = await ReadForeignKeysAsync(context, "TeachingNoteBindings");
        Assert.Contains(
            teachingNoteBindingForeignKeys,
            foreignKey => foreignKey.Table == "TeachingNotes"
                && foreignKey.From == "TeachingNoteId"
                && foreignKey.OnDelete == "RESTRICT");

        var sectionItemForeignKeys = await ReadForeignKeysAsync(context, "SectionItems");
        Assert.Contains(
            sectionItemForeignKeys,
            foreignKey => foreignKey.Table == "Sections"
                && foreignKey.From == "SectionId"
                && foreignKey.OnDelete == "RESTRICT");
        Assert.DoesNotContain(sectionItemForeignKeys, foreignKey => foreignKey.From == "TargetId");

        var handoutVersionItemForeignKeys = await ReadForeignKeysAsync(context, "HandoutVersionItems");
        Assert.Contains(
            handoutVersionItemForeignKeys,
            foreignKey => foreignKey.Table == "HandoutVersions"
                && foreignKey.From == "HandoutVersionId"
                && foreignKey.OnDelete == "RESTRICT");
        Assert.DoesNotContain(handoutVersionItemForeignKeys, foreignKey => foreignKey.From == "TargetId");
    }

    [Fact]
    public async Task Handout_version_item_target_type_constraint_allows_atomic_section_but_rejects_section()
    {
        var databasePath = Path.Combine(CreateTempDirectory(), "cms-v2.db");

        await using var context = CmsV2DbContextFactory.CreateForDatabase(databasePath);
        await context.Database.MigrateAsync();

        var createTableSql = await ReadCreateTableSqlAsync(context, "HandoutVersionItems");

        Assert.Contains("\"TargetType\" IN (1, 2, 4)", createTableSql, StringComparison.Ordinal);
        Assert.DoesNotContain("\"TargetType\" IN (1, 2)", createTableSql, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AddPreClassQuizPanels_migration_backfills_missing_empty_panels_without_duplicates()
    {
        var databasePath = Path.Combine(CreateTempDirectory(), "cms-v2.db");

        await using var context = CmsV2DbContextFactory.CreateForDatabase(databasePath);
        var migrator = context.GetService<IMigrator>();

        await migrator.MigrateAsync("20260624171728_RebuildTeachingNotes");
        await SeedAtomicSectionsForPreClassQuizBackfillAsync(context);

        await migrator.MigrateAsync();

        var panels = await ReadAtomicSectionPanelsAsync(context);

        var firstQuizPanel = Assert.Single(panels, panel => panel.AtomicSectionId == 1 && panel.TeachingRole == 6);
        Assert.Equal("AS Needs Quiz", firstQuizPanel.Title);
        Assert.Equal(2, firstQuizPanel.Difficulty);
        Assert.Equal(40, firstQuizPanel.SortOrder);

        var noPanelQuizPanel = Assert.Single(panels, panel => panel.AtomicSectionId == 2 && panel.TeachingRole == 6);
        Assert.Equal("AS Without Panels", noPanelQuizPanel.Title);
        Assert.Equal(3, noPanelQuizPanel.Difficulty);
        Assert.Equal(40, noPanelQuizPanel.SortOrder);

        var existingQuizPanel = Assert.Single(panels, panel => panel.AtomicSectionId == 3 && panel.TeachingRole == 6);
        Assert.Equal("Existing PreClassQuiz", existingQuizPanel.Title);
        Assert.Equal(0, existingQuizPanel.Difficulty);
        Assert.Equal(99, existingQuizPanel.SortOrder);

        var duplicatedPreClassQuizPanels = await ReadScalarIntAsync(
            context,
            """
            SELECT COUNT(*)
            FROM (
                SELECT AtomicSectionId
                FROM AtomicSectionPanels
                WHERE TeachingRole = 6
                GROUP BY AtomicSectionId
                HAVING COUNT(*) > 1
            );
            """);

        Assert.Equal(0, duplicatedPreClassQuizPanels);
        Assert.Equal(0, await ReadScalarIntAsync(context, "SELECT COUNT(*) FROM ContentBlocks;"));
    }

    private static string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "cms-v2-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private static async Task<HashSet<string>> ReadScalarStringsAsync(CmsV2DbContext context, string sql)
    {
        var results = new HashSet<string>(StringComparer.Ordinal);
        var connection = context.Database.GetDbConnection();

        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        await using var command = connection.CreateCommand();
        command.CommandText = sql;

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            results.Add(reader.GetString(0));
        }

        return results;
    }

    private static async Task<HashSet<string>> ReadTableColumnsAsync(CmsV2DbContext context, string tableName)
    {
        var columns = new HashSet<string>(StringComparer.Ordinal);
        var connection = context.Database.GetDbConnection();

        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        await using var command = connection.CreateCommand();
        command.CommandText = $"PRAGMA table_info({tableName});";

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            columns.Add(reader.GetString(1));
        }

        return columns;
    }

    private static async Task<IReadOnlyList<SqliteIndex>> ReadIndexesAsync(CmsV2DbContext context, string tableName)
    {
        var indexes = new List<SqliteIndex>();
        var connection = context.Database.GetDbConnection();

        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        await using var command = connection.CreateCommand();
        command.CommandText = $"PRAGMA index_list({tableName});";

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            indexes.Add(new SqliteIndex(
                reader.GetString(1),
                reader.GetInt32(2) == 1,
                []));
        }

        var hydratedIndexes = new List<SqliteIndex>();
        foreach (var index in indexes)
        {
            hydratedIndexes.Add(index with { Columns = await ReadIndexColumnsAsync(context, index.Name) });
        }

        return hydratedIndexes;
    }

    private static async Task<IReadOnlyList<string>> ReadIndexColumnsAsync(CmsV2DbContext context, string indexName)
    {
        var columns = new List<string>();
        var connection = context.Database.GetDbConnection();

        await using var command = connection.CreateCommand();
        command.CommandText = $"PRAGMA index_info({indexName});";

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            columns.Add(reader.GetString(2));
        }

        return columns;
    }

    private static async Task<IReadOnlyList<SqliteForeignKey>> ReadForeignKeysAsync(CmsV2DbContext context, string tableName)
    {
        var foreignKeys = new List<SqliteForeignKey>();
        var connection = context.Database.GetDbConnection();

        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        await using var command = connection.CreateCommand();
        command.CommandText = $"PRAGMA foreign_key_list({tableName});";

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            foreignKeys.Add(new SqliteForeignKey(
                reader.GetString(2),
                reader.GetString(3),
                reader.GetString(6)));
        }

        return foreignKeys;
    }

    private static async Task<string> ReadCreateTableSqlAsync(CmsV2DbContext context, string tableName)
    {
        var connection = context.Database.GetDbConnection();

        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT sql FROM sqlite_master WHERE type = 'table' AND name = $tableName;";

        var parameter = command.CreateParameter();
        parameter.ParameterName = "$tableName";
        parameter.Value = tableName;
        command.Parameters.Add(parameter);

        var result = await command.ExecuteScalarAsync();

        return Assert.IsType<string>(result);
    }

    private static async Task SeedAtomicSectionsForPreClassQuizBackfillAsync(CmsV2DbContext context)
    {
        const string updatedTime = "2026-06-26T00:00:00.0000000+08:00";

        await ExecuteNonQueryAsync(
            context,
            $"""
            INSERT INTO TeachingTopics (Id, ParentId, Name, Description, SortOrder, Status, UpdatedTime)
            VALUES
                (1, NULL, 'Topic A', NULL, 10, 1, '{updatedTime}'),
                (2, NULL, 'Topic B', NULL, 20, 1, '{updatedTime}'),
                (3, NULL, 'Topic C', NULL, 30, 1, '{updatedTime}');
            """);

        await ExecuteNonQueryAsync(
            context,
            $"""
            INSERT INTO Sections (Id, TeachingTopicId, Title, Description, Type, Difficulty, Status, SortOrder, UpdatedTime)
            VALUES
                (1, 1, 'Section A', NULL, 1, 2, 1, 10, '{updatedTime}'),
                (2, 2, 'Section B', NULL, 1, 3, 1, 20, '{updatedTime}'),
                (3, 3, 'Section C', NULL, 1, 1, 1, 30, '{updatedTime}');
            """);

        await ExecuteNonQueryAsync(
            context,
            $"""
            INSERT INTO AtomicSections (Id, SectionId, Title, Description, Type, Difficulty, Status, UpdatedTime)
            VALUES
                (1, 1, 'AS Needs Quiz', NULL, 1, 2, 1, '{updatedTime}'),
                (2, 2, 'AS Without Panels', NULL, 1, 3, 1, '{updatedTime}'),
                (3, 3, 'AS Already Has Quiz', NULL, 1, 1, 1, '{updatedTime}');
            """);

        await ExecuteNonQueryAsync(
            context,
            $"""
            INSERT INTO AtomicSectionPanels (Id, AtomicSectionId, Title, TeachingRole, Difficulty, SortOrder, UpdatedTime)
            VALUES
                (1, 1, 'Knowledge', 1, 2, 10, '{updatedTime}'),
                (2, 1, 'Example', 2, 2, 20, '{updatedTime}'),
                (3, 1, 'Variant', 3, 2, 30, '{updatedTime}'),
                (4, 3, 'Existing PreClassQuiz', 6, 0, 99, '{updatedTime}');
            """);
    }

    private static async Task ExecuteNonQueryAsync(CmsV2DbContext context, string sql)
    {
        var connection = context.Database.GetDbConnection();

        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync();
    }

    private static async Task<int> ReadScalarIntAsync(CmsV2DbContext context, string sql)
    {
        var connection = context.Database.GetDbConnection();

        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        await using var command = connection.CreateCommand();
        command.CommandText = sql;

        var result = await command.ExecuteScalarAsync();

        return Convert.ToInt32(result);
    }

    private static async Task<IReadOnlyList<AtomicSectionPanelRow>> ReadAtomicSectionPanelsAsync(CmsV2DbContext context)
    {
        var rows = new List<AtomicSectionPanelRow>();
        var connection = context.Database.GetDbConnection();

        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT Id, AtomicSectionId, Title, TeachingRole, Difficulty, SortOrder
            FROM AtomicSectionPanels
            ORDER BY AtomicSectionId, SortOrder, Id;
            """;

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            rows.Add(new AtomicSectionPanelRow(
                reader.GetInt32(0),
                reader.GetInt32(1),
                reader.GetString(2),
                reader.GetInt32(3),
                reader.GetInt32(4),
                reader.GetInt32(5)));
        }

        return rows;
    }

    private sealed record SqliteIndex(string Name, bool IsUnique, IReadOnlyList<string> Columns);

    private sealed record SqliteForeignKey(string Table, string From, string OnDelete);

    private sealed record AtomicSectionPanelRow(
        int Id,
        int AtomicSectionId,
        string Title,
        int TeachingRole,
        int Difficulty,
        int SortOrder);
}
