namespace Web.Request;

public record AttendanceRequest(
    Guid ConsultationId,
    string UserId,
    Guid RoomId,
    string? Comment
);