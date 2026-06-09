using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WordSolution.CmsV2.Infrastructure.Persistence;

public sealed class CmsV2DesignTimeDbContextFactory : IDesignTimeDbContextFactory<CmsV2DbContext>
{
    public CmsV2DbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<CmsV2DbContext>()
            .UseSqlite($"Data Source={CmsV2DatabasePaths.DatabaseFileName}")
            .Options;

        return new CmsV2DbContext(options);
    }
}
