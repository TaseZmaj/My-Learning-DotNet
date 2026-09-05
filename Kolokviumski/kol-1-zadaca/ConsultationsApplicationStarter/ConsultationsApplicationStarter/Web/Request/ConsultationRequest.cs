namespace Web.Request;

public record ConsultationRequest(
    DateTime StartTime, 
    DateTime EndTime,
    Guid RoomId
);