using EventsManagement.Domain.Dto;
using EventsManagement.Domain.Enums;

namespace EventsManagement.Web.Response;

public record EventResponse(
    string Name,
    string Description,
    string ImageUrl,
    DateTime StartDate,
    DateTime EndDate,
    string Status,
        
    //BITNO:ova e toa shto clientot treba da go display-ne, vo EventResponse.cs ima VenueId bidejki toa e del od requestot
    string? VenueName, 
    string? VenueCity,
    string? VenueCountry,
    
    string CreatedById,
    DateTime DateCreated,
    string? LastModifiedById,
    DateTime? DateLastModified,
    
    EventWeatherDto? WeatherData
);