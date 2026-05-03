using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Service.Implementations;

namespace Service.jobs;

public class LegacyDBEtlBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger _logger;

    public LegacyDBEtlBackgroundService(IServiceScopeFactory serviceScopeFactory, ILogger<LegacyDBEtlBackgroundService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceScopeFactory.CreateScope();

            var service = scope.ServiceProvider.GetRequiredService<VenueETLService>();

            try
            {
                _logger.LogInformation("Starting Legacy DB ETL job");

                await service.SyncAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during legacy DB ETL job");
            }
            
            await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
        }
    }
}