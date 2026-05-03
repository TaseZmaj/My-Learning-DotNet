using Microsoft.Extensions.Logging;
using Quartz;
using Service.Interfaces;

namespace Service.jobs;

[DisallowConcurrentExecution] //Ova go imashe vo prezentacijata, ALI NE VO gitlab kodot
public class QuartzReservationCleanupJob : IJob
{
    private readonly IReservationService _reservationService;
    private readonly ILogger _logger;

    public QuartzReservationCleanupJob(IReservationService reservationService, ILogger<QuartzReservationCleanupJob> logger)
    {
        _reservationService = reservationService;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Reservation cleanup job started...");

        var reservations = await _reservationService.GetAllByDateReservedSince(DateTime.Now.AddMinutes(-15));

        _logger.LogInformation($"Fetched total {reservations.Count} reservations");

        foreach (var reservation in reservations)
        {
            await _reservationService.ExpireAsync(reservation);
            _logger.LogInformation($"Reservation {reservation.Id} has been cleared");
        }
        
        _logger.LogInformation("Reservation cleanup job finished...");
    }
}