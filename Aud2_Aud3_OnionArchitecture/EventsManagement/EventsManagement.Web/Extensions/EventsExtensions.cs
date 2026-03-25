using EventsManagement.Domain.Dto;
using EventsManagement.Domain.Entities;
using EventsManagement.Web.Request;
using EventsManagement.Web.Response;

namespace EventsManagement.Web.Extensions;

public static class EventsExtensions
{
    public static EventResponse? ToResponse(this Event e)
    {
        return new EventResponse(
            e.Title,
            e.Description,
            e.ImageUrl,
            e.StartDate,
            e.EndDate,
            e.Venue?.Name,
            e.Venue?.City,
            e.Venue?.Country
        );
    }
    
    public static List<EventResponse?> ToResponse(this List<Event> events)
    {
        // return events.Select(ToResponse).ToList();
        return events.Select(x => x.ToResponse()).ToList();
    }
    
    public static EventDto ToDto(this EventRequest request)
    {
        return new EventDto
        {
            Title = request.Title,
            Description = request.Description,
            ImageUrl = request.ImageUrl,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            // VenueId = request.VenueId,
            // UserId = request.UserId,
        };
    }
}