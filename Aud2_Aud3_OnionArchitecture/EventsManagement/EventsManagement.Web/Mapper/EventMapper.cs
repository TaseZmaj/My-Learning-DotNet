using EventsManagement.Web.Response;
using Service.Interfaces;
using EventsManagement.Web.Extensions;
using EventsManagement.Web.Request;

namespace EventsManagement.Web.Mapper;

public class EventMapper
{
    private readonly IEventService _eventService;

    public EventMapper(IEventService eventService)
    {
        _eventService = eventService;
    }
    
    public async Task<EventResponse?> GetById(Guid id)
    {
        var result = await _eventService.GetByIdAsync(id);
        return result.ToResponse();
    }

    public async Task<List<EventResponse>> GetAll()
    {
        var result = await _eventService.GetAllEventsAsyncWithEventPricingUsingInclude();
        return result.ToResponse();
    }

    public async Task<PaginatedResponse<EventResponse>> PaginatedGetAllAsync(PaginateRequest request)
    {
        var result = await _eventService.GetAllPagedAsync(request.PageNumber, request.PageSize);
        return result.ToPaginatedResponse(e => e.ToResponse());
    }

    public async Task<EventResponse> InsertAsync(EventRequest request)
    {
        var dto = request.ToDto();
        
        var result = await _eventService.InsertAsync(dto);
        
        return result.ToResponse();
    }

    public async Task<EventResponse> UpdateAsync(Guid id, EventRequest request)
    {
        var dto = request.ToDto();
        var result = await _eventService.UpdateAsync(id, dto);
        return result.ToResponse();
    }

    public async Task<EventResponse> DeleteAsync(Guid id)
    {
        var result = await _eventService.DeleteAsync(id);
        return result?.ToResponse();
    }
}