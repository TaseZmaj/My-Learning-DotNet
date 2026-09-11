using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Service.Implementation;
using Service.Interface;

namespace Service.Jobs;

public class EtlSyncBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<EtlSyncBackgroundService> _logger;

    public EtlSyncBackgroundService(IServiceScopeFactory serviceScopeFactory, ILogger<EtlSyncBackgroundService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceScopeFactory.CreateScope();

            var service = scope.ServiceProvider.GetRequiredService<IEtlSyncService>();

            try
            {
                _logger.LogInformation("Starting Consultations API ETL sync");

                await service.SyncAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Consultations API ETL sync");
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}