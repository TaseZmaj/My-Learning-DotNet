using Service.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Service.jobs;

//Pros: Zero extra dependencies; very easy to understand; perfect for simple,
//frequent tasks (like your 5-minute cleanup).
//Cons: If the app restarts, the timer resets. It is difficult to schedule complex
//patterns (e.g., "Run only on the last Friday of every month at 2 AM").
public class ReservationCleanupBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<ReservationCleanupBackgroundService> _logger;

    public ReservationCleanupBackgroundService(IServiceScopeFactory serviceScopeFactory,
        ILogger<ReservationCleanupBackgroundService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var reservationService = scope.ServiceProvider.GetRequiredService<IReservationService>();
            
            _logger.LogInformation("Reservation cleanup job started...");
            
            var reservations = await reservationService.GetAllByDateReservedSince(DateTime.UtcNow.AddMinutes(-15));

            _logger.LogInformation("Fetched total {reservationCount} reservations", reservations.Count);

            foreach (var reservation in reservations)
            {
                try
                {
                    _logger.LogInformation("Expiring reservation with ID: {reservationId}", reservation.Id);
                    await reservationService.ExpireAsync(reservation);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while expiring reservation with ID {reservationId}", reservation.Id);
                }
            }
            
            _logger.LogInformation("Reservation cleanup job finished succesfully...");
            
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}