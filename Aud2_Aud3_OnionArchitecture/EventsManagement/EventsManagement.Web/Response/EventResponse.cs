namespace EventsManagement.Web.Response;

public record EventResponse(
    string Name,
    string Description,
    string ImageUrl,
    DateTime StartDate,
    DateTime EndDate,
        
    //BITNO:ova e toa shto clientot treba da go display-ne, vo EventResponse.cs ima VenueId bidejki toa e del od requestot
    string? VenueName, 
    string? VenueCity,
    string? VenueCountry
);