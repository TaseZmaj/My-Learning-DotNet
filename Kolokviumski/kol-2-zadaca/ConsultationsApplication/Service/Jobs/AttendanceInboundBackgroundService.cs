using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Service.Implementation;
using Service.Interface;

namespace Service.Jobs;

public class AttendanceInboundBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<AttendanceInboundBackgroundService> _logger;

    public AttendanceInboundBackgroundService(
        IServiceScopeFactory serviceScopeFactory, 
        ILogger<AttendanceInboundBackgroundService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceScopeFactory.CreateScope();

            var service = scope.ServiceProvider.GetRequiredService<IInboundEventEntryProcessor>();

            try
            {
                _logger.LogInformation("Starting Legacy DB ETL job");

                await service.ProcessPendingEventsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during legacy DB ETL job");
            }
            
            await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
        }
    }
}