namespace Web.Response;

public record ConsultationResponse(
    Guid Id,
    DateOnly Date,
    Guid RoomId,
    int RegisteredStudents,
    List<AttendanceBasicResponse> Attendances
);