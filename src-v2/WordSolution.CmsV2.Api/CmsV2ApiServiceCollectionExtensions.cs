using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WordSolution.CmsV2.Application.AtomicSections;
using WordSolution.CmsV2.Application.ContentBlocks;
using WordSolution.CmsV2.Application.Handouts;
using WordSolution.CmsV2.Application.SectionVariants;
using WordSolution.CmsV2.Application.Sections;
using WordSolution.CmsV2.Application.TeachingStructure;
using WordSolution.CmsV2.Domain.Documents;
using WordSolution.CmsV2.Domain.Repositories;
using WordSolution.CmsV2.Infrastructure.Documents;
using WordSolution.CmsV2.Infrastructure.Persistence;
using WordSolution.CmsV2.Infrastructure.Repositories;

namespace WordSolution.CmsV2.Api;

public static class CmsV2ApiServiceCollectionExtensions
{
    public static IServiceCollection AddCmsV2Api(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<CmsV2ApiOptions>(configuration.GetSection(CmsV2ApiOptions.SectionName));
        services.PostConfigure<CmsV2ApiOptions>(options =>
        {
            if (string.IsNullOrWhiteSpace(options.BankRootDirectory))
            {
                options.BankRootDirectory = new CmsV2ApiOptions().BankRootDirectory;
            }
        });

        services.AddDbContext<CmsV2DbContext>((serviceProvider, options) =>
        {
            var apiOptions = serviceProvider.GetRequiredService<IOptions<CmsV2ApiOptions>>().Value;
            Directory.CreateDirectory(apiOptions.BankRootDirectory);
            options.UseSqlite($"Data Source={CmsV2DatabasePaths.GetDatabasePath(apiOptions.BankRootDirectory)}");
        });

        services.AddScoped<ICmsV2UnitOfWork, EfCmsV2UnitOfWork>();
        services.AddSingleton<ICmsV2FileAssetPathProvider, CmsV2FileAssetPathProvider>();
        services.AddSingleton<IContentBlockFileStore, LocalContentBlockFileStore>();
        services.AddSingleton<IContentBlockDocumentProcessor, AsposeContentBlockDocumentProcessor>();
        services.AddSingleton<IContentBlockEditSessionStore, LocalContentBlockEditSessionStore>();
        services.AddSingleton<IContentBlockEditSessionFileStore, LocalContentBlockEditSessionFileStore>();
        services.AddSingleton<IContentBlockEditSessionLauncher, LocalContentBlockEditSessionLauncher>();
        services.AddSingleton<IHandoutDocumentGenerator, AsposeHandoutDocumentGenerator>();

        services.AddScoped<ContentBlockUseCases>();
        services.AddScoped<ContentBlockDocumentUseCases>();
        services.AddScoped<ContentBlockEditSessionUseCases>();
        services.AddScoped<ContentBlockRelationUseCases>();
        services.AddScoped<SectionUseCases>();
        services.AddScoped<AtomicSectionUseCases>();
        services.AddScoped<SectionVariantUseCases>();
        services.AddScoped<TeachingStructureUseCases>();
        services.AddScoped<HandoutUseCases>();
        services.AddScoped<HandoutGenerationUseCases>();
        services.AddHostedService<ContentBlockEditSessionBackgroundService>();

        return services;
    }

    public static async Task InitializeCmsV2DatabaseAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<CmsV2ApiOptions>>().Value;
        Directory.CreateDirectory(options.BankRootDirectory);

        var context = scope.ServiceProvider.GetRequiredService<CmsV2DbContext>();
        await context.Database.MigrateAsync();
    }
}
