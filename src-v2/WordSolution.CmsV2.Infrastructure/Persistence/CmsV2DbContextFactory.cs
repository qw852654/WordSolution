using Microsoft.EntityFrameworkCore;

namespace WordSolution.CmsV2.Infrastructure.Persistence;

public static class CmsV2DbContextFactory
{
    public static CmsV2DbContext CreateForBankRoot(string bankRootDirectory)
    {
        return CreateForDatabase(CmsV2DatabasePaths.GetDatabasePath(bankRootDirectory));
    }

    public static CmsV2DbContext CreateForDatabase(string databasePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databasePath);

        var directory = Path.GetDirectoryName(Path.GetFullPath(databasePath));
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var options = new DbContextOptionsBuilder<CmsV2DbContext>()
            .UseSqlite($"Data Source={databasePath}")
            .Options;

        return new CmsV2DbContext(options);
    }
}
