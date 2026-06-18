using Microsoft.Extensions.Options;
using WordSolution.CmsV2.Application.ContentBlocks;

namespace WordSolution.CmsV2.Api;

public sealed class ContentBlockEditSessionBackgroundService : BackgroundService
{
    private static readonly TimeSpan ScanInterval = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan MinimumSessionAge = TimeSpan.FromSeconds(10);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptions<CmsV2ApiOptions> _options;
    private readonly ILogger<ContentBlockEditSessionBackgroundService> _logger;

    public ContentBlockEditSessionBackgroundService(
        IServiceScopeFactory scopeFactory,
        IOptions<CmsV2ApiOptions> options,
        ILogger<ContentBlockEditSessionBackgroundService> logger)
    {
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(ScanInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            await SyncActiveSessionsAsync(stoppingToken);

            try
            {
                if (!await timer.WaitForNextTickAsync(stoppingToken))
                {
                    break;
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    private async Task SyncActiveSessionsAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var useCases = scope.ServiceProvider.GetRequiredService<ContentBlockEditSessionUseCases>();
            await useCases.SyncActiveSessionsAsync(
                new SyncActiveContentBlockEditSessionsCommand(
                    _options.Value.BankRootDirectory,
                    MinimumSessionAge),
                cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "ContentBlock edit session auto sync failed.");
        }
    }
}
