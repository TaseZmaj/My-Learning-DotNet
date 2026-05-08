using EventsManagement.Domain.Dto;
using EventsManagement.Domain.Entities;

namespace Service.Interfaces;

public interface IEventService
{
    public Task<List<Event>> GetAllAsync();
    public Task<Event?> GetByIdAsync(Guid id);

    public Task<Event?> GetByTitleVenueIdAndStartDate(string title, Guid venueId, DateTime StartDate);
    public Task<Event> GetByIdNotNullAsync(Guid id);
    public Task<Event> InsertAsync(EventDto dto);
    public Task<Event> UpdateAsync(Guid id, EventDto dto);
    public Task<Event> DeleteAsync(Guid id);
    
    
    public Task<PaginatedResult<Event>> GetAllPagedAsync(int pageNumber, int pageSize);
    
    // Get ALL events with included event pricing, section and venue
    Task<List<Event>> GetAllEventsAsyncWithEventPricingWithoutInclude();
    Task<List<Event>> GetAllEventsAsyncWithEventPricingUsingInclude();
    
    
    //For image uploads
    public Task<Event> UploadImageByIdAsync(Guid eventId, string fileName, string contentType, int size, byte[] data);
    public Task<Event> UpdateImagePathByIdAsync(Guid eventId, string path);
}