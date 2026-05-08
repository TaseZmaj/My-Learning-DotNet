using System.Text.Json;
using EventsManagement.Domain.Dto;
using EventsManagement.Domain.Entities;
using EventsManagement.Domain.Enums;
using EventsManagement.Repository.Interfaces;
using Microsoft.Extensions.Logging;
using Service.Interfaces;

namespace Service.Implementations;

public class InboundEventService : IInboundEventService
{
    private readonly IRepository<InboundEventEntry> _repository;
    private readonly ILogger _logger;
    
    public InboundEventService(
        IRepository<InboundEventEntry> repository,
        ILogger<InboundEventService> logger
        )
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<InboundEventEntry> QueueAsync(InboundEventRequest request, ApiClient apiClient)
    {
        var entry = new InboundEventEntry
        {
            Id = Guid.NewGuid(),
            RawPayload = JsonSerializer.Serialize(request),
            Status = InboundEventStatus.Pending,
            ApiClientId = apiClient.Id,
            ReceivedAt = DateTime.UtcNow
        };
        
        await _repository.InsertAsync(entry);
        
        _logger.LogInformation(
            $"Event from {apiClient.Name}: {request.Title}, queued as {entry.Id}"
            );
        
        return entry;
    }
    
    public async Task<InboundEventEntry?> GetStatusByIdAsync(Guid id)
    {
        return await _repository.GetAsync(
            selector: x => x, 
            x => x.Id == id);
    }
}