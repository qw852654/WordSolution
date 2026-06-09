namespace WordSolution.CmsV2.Infrastructure.Persistence;

public static class CmsV2DatabasePaths
{
    public const string DatabaseFileName = "cms-v2.db";

    public static string GetDatabasePath(string bankRootDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(bankRootDirectory);

        return Path.Combine(bankRootDirectory, DatabaseFileName);
    }
}
