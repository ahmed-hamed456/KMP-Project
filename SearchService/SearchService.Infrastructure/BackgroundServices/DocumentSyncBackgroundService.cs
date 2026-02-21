using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SearchService.Application.Interfaces;

namespace SearchService.Infrastructure.BackgroundServices;

public class DocumentSyncBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DocumentSyncBackgroundService> _logger;
    private readonly TimeSpan _syncInterval = TimeSpan.FromMinutes(5);

    public DocumentSyncBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<DocumentSyncBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Document Sync Background Service is starting");

        // Initial sync on startup
        await PerformSyncAsync(stoppingToken);

        // Periodic sync
        using var timer = new PeriodicTimer(_syncInterval);

        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            await PerformSyncAsync(stoppingToken);
        }

        _logger.LogInformation("Document Sync Background Service is stopping");
    }

    private async Task PerformSyncAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Starting periodic document sync at {Time}", DateTime.UtcNow);

            using var scope = _serviceProvider.CreateScope();
            var syncService = scope.ServiceProvider.GetRequiredService<IDocumentSyncService>();

            await syncService.SyncDocumentsAsync();

            _logger.LogInformation("Periodic document sync completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during periodic document sync");
            // Don't throw - just log and continue
        }
    }
}
