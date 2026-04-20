namespace EventsManagement.Web.Response;

public record ReservationResponse(
    string? UserName,
    string? EventName,
    Guid EventId,
    Guid Id,
    DateTime ReservationDate,
    DateTime? DateLastModified
);



public record ReservationBasicResponse(
    Guid EventId,
    Guid Id,
    DateTime ReservationDate,
    DateTime? DateLastModified
);