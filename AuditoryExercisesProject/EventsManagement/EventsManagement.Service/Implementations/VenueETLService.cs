using EventsManagement.Domain.Entities;
using EventsManagement.Repository.Interfaces;
using Microsoft.Extensions.Logging;

namespace Service.Implementations;

public class VenueETLService
{
    private readonly ILegacyVenueRepository _legacyVenueRepository;
    private readonly IVenueRepository _venueRepository;
    private readonly IRepository<EtlSyncLog> _etlSyncLogRepository;
    private readonly ILogger _logger;

    public VenueETLService(
        ILegacyVenueRepository legacyVenueRepository, 
        IVenueRepository venueRepository, 
        IRepository<EtlSyncLog> etlSyncLogRepository, 
        ILogger<VenueETLService> logger)
    {
        _legacyVenueRepository = legacyVenueRepository;
        _venueRepository = venueRepository;
        _etlSyncLogRepository = etlSyncLogRepository;
        _logger = logger;
    }

    public async Task SyncAllAsync()
    {
        var syncLog = new EtlSyncLog
        {
            JobName = "VenueSync",
            StartedAt = DateTime.UtcNow
        };

        try
        {
            var lastRun = await _etlSyncLogRepository.GetAllAsync(
                selector: x => x,
                predicate: x => x.JobName == "VenueSync" && x.Success == true,
                orderBy: x => x.OrderByDescending(v => v.StartedAt));

            var date = lastRun.FirstOrDefault()?.StartedAt ?? DateTime.MinValue;

            _logger.LogInformation("Starting Legacy DB ETL with date last modified {date}", date);

            var venues = await _legacyVenueRepository.GetVenuesModifiedSinceAsync(date);
            var sections = await _legacyVenueRepository.GetSectionsModifiedSinceAsync(date);
            var seats = await _legacyVenueRepository.GetSeatsModifiedSinceAsync(date);

            _logger.LogInformation(
                "Extracted and transformed total {venues} Venues, {sections}  Sections, {seats} Seats", venues.Count,
                sections.Count, seats.Count);

            await _venueRepository.BulkInsertOrUpdateVenuesAsync(venues);
            await _venueRepository.BulkInsertOrUpdateSectionsAsync(sections);
            await _venueRepository.BulkInsertOrUpdateSeatsAsync(seats);
            
            _logger.LogInformation("Successfully loaded the data");

            syncLog.Success = true;
            syncLog.CompletedAt = DateTime.UtcNow;
            
            
            _logger.LogInformation("Legacy DB ETL finished successfully at {date}", syncLog.CompletedAt);

        }
        catch (Exception ex)
        {
            syncLog.Success = false;
            syncLog.ErrorMessage = ex.Message;
            syncLog.CompletedAt = DateTime.UtcNow;
            _logger.LogError(ex, "An error occured during the ETL process...");
        }
        finally
        {
            await _etlSyncLogRepository.InsertAsync(syncLog);
        }
    }
}