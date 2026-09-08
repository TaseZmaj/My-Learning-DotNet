namespace Web.Request;

public record ConsultationRequest(
    Guid RoomId,
    DateTime StartTime,
    DateTime EndTime
);
