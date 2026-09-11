using Domain.Dto;
using Domain.Enums;
using Domain.ExternalModels;
using Domain.Models;
using Microsoft.Extensions.Logging;
using Repository.Interface;
using Service.Interface;

namespace Service.Implementation;

//za sync-anje na consultations-ot
public class EtlSyncService : IEtlSyncService
{
    private readonly IConsultationsRepository _consultationsRepository;
    private readonly IRepository<Room> _roomRepository;
    private readonly IRepository<EtlSyncLog> _etlSyncLogRepository;
    private readonly ILogger<EtlSyncService> _logger;
    private readonly IConsultationsApiClient<ExternalConsultationsDto> _consultationsApiClient;

    public EtlSyncService(IConsultationsRepository consultationsRepository, IRepository<Room> roomRepository, IRepository<EtlSyncLog> etlSyncLogRepository, ILogger<EtlSyncService> logger, IConsultationsApiClient<ExternalConsultationsDto> consultationsApiClient)
    {
        _consultationsRepository = consultationsRepository;
        _roomRepository = roomRepository;
        _etlSyncLogRepository = etlSyncLogRepository;
        _logger = logger;
        _consultationsApiClient = consultationsApiClient;
    }

    public async Task SyncAllAsync()
    {
        var syncLog = new EtlSyncLog
        {
            JobName = "ConsultationsSync",
            StartedAt = DateTime.UtcNow
        };

        try
        {
            var lastRun = await _etlSyncLogRepository.GetAllAsync(
                selector: x => x,
                predicate: x => x.JobName == "ConsultationsSync" && x.Success == true,
                orderBy: x => x.OrderByDescending(v => v.StartedAt));

            var date = lastRun.FirstOrDefault()?.StartedAt ?? DateTime.MinValue;

            _logger.LogInformation("Starting Consultations Api DB ETL with date last modified {date}", date);

            var apiData = await _consultationsApiClient.GetAllConsultationsModifiedSinceAsync(date);
            var rooms = await _roomRepository.GetAllAsync(x => x);
            var roomsByName = rooms.ToDictionary(x => x.Name, x => x.Id);
            
            var consultations = apiData.Items.Select(x =>
                new Consultation
                {
                    Id = GuidHelper.FromLegacyId("Consultation", x.ExternalId),
                    StartTime = x.StartTime,
                    EndTime = x.EndTime,
                    RoomId = roomsByName[x.RoomName]
                }
            ).ToList();
            
            _logger.LogInformation(
                "Extracted and transformed total {sections} sections", consultations.Count());

            await _consultationsRepository.BulkInsertOrUpdateAsync(consultations);
            
            
            _logger.LogInformation("Successfully loaded the data");

            syncLog.Success = true;
            syncLog.CompletedAt = DateTime.UtcNow;
            
            _logger.LogInformation("Consultations API ETL finished successfully at {date}", syncLog.CompletedAt);

        }
        catch (Exception ex)
        {
            syncLog.Success = false;
            syncLog.ErrorMessage = ex.Message;
            syncLog.CompletedAt = DateTime.UtcNow;
            _logger.LogError(ex, "An error occured during the Consultations API ETL process...");
        }
        finally
        {
            await _etlSyncLogRepository.InsertAsync(syncLog);
        }
    }
}