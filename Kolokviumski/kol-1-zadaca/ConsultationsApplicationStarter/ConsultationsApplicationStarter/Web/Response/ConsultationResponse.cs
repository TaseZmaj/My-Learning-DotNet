using Domain.Models;

namespace Web.Response;

public record ConsultationResponse(
    Guid Id,
    DateOnly Date,
    Guid RoomId,
    // string RoomName,
    int RegisteredStudents,
    List<BasicAttendanceResponse> Attendances
);