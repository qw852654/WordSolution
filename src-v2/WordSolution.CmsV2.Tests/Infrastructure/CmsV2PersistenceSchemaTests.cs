using System.Data;
using Microsoft.EntityFrameworkCore;
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
        "ContentBlockRelations",
        "Handouts",
        "HandoutVersions",
        "HandoutVersionItems",
        "OutputTemplates",
        "OutputForms",
        "GeneratedFiles",
        "TeachingNotes"
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
            Assert.DoesNotContain("CreatedTime", columns);
        }

        var generatedFileColumns = await ReadTableColumnsAsync(context, "GeneratedFiles");
        Assert.Contains("GeneratedTime", generatedFileColumns);

        var contentBlockVersionColumns = await ReadTableColumnsAsync(context, "ContentBlockVersions");
        Assert.DoesNotContain("Difficulty", contentBlockVersionColumns);
        Assert.DoesNotContain("BlockType", contentBlockVersionColumns);
        Assert.DoesNotContain("QuestionType", contentBlockVersionColumns);

        var atomicSectionPanelColumns = await ReadTableColumnsAsync(context, "AtomicSectionPanels");
        Assert.Contains("AtomicSectionId", atomicSectionPanelColumns);
        Assert.Contains("Title", atomicSectionPanelColumns);
        Assert.Contains("TeachingRole", atomicSectionPanelColumns);
        Assert.Contains("Difficulty", atomicSectionPanelColumns);
        Assert.Contains("SortOrder", atomicSectionPanelColumns);

        var atomicSectionItemColumns = await ReadTableColumnsAsync(context, "AtomicSectionItems");
        Assert.Contains("AtomicSectionPanelId", atomicSectionItemColumns);
        Assert.Contains("TeachingRole", atomicSectionItemColumns);
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

    private sealed record SqliteIndex(string Name, bool IsUnique, IReadOnlyList<string> Columns);

    private sealed record SqliteForeignKey(string Table, string From, string OnDelete);
}
