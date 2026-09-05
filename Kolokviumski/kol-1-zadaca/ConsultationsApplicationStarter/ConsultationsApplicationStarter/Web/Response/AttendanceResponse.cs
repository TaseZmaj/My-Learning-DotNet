using Domain.Enums;

namespace Web.Response;

public record AttendanceResponse(
    Guid Id,
    string UserId,
    string FirstName,
    string LastName,
    Status Status,
    string? Comment
);