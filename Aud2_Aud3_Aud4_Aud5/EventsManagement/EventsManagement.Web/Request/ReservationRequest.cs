namespace EventsManagement.Web.Request;

public record ReservationRequest(
    Guid EventId
);
    
public record ReservationWithUserRequest(
    Guid EventId,
    string UserId
);