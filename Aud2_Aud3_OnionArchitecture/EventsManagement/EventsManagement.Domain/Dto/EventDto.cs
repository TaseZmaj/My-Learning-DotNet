using EventsManagement.Domain.Enums;

namespace EventsManagement.Domain.Dto;

public class EventDto
{
    public string Title { get; set; }
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public EventStatus Status { get; set; }
    public string? ImageUrl { get; set; }
    
    public Guid VenueId { get; set; }
    public string UserId { get; set; }
}