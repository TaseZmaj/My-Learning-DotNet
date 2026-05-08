namespace EventsManagement.Domain.Dto;

public record InboundEventRequest(
    string Title, 
    string? Description,
    DateTime StartDate,
    DateTime EndDate,
    string VenueName,
    string? VenueCity,
    List<InboundSectionPricing> Pricing
);

public record InboundSectionPricing(
    string SectionName,
    decimal Price,
    string Currency = "USD"
);

