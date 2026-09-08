namespace Web.Response;

public record AttendanceResponse(
    string UserId,
    string FirstName,
    string LastName,
    string Status,
    string? Comment
);