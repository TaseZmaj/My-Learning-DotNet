using EventsManagement.Domain.Dto;
using EventsManagement.Domain.Entities;

namespace Service.Interfaces;

public interface IInboundEventService
{
    Task<InboundEventEntry> QueueAsync(InboundEventRequest inboundEventRequest, ApiClient apiClient);
    
    Task<InboundEventEntry?> GetStatusByIdAsync(Guid id);
}