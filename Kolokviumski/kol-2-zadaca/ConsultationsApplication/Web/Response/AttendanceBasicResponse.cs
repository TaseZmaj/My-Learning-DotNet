namespace Web.Response;

public record AttendanceBasicResponse(
    Guid Id,
    string FirstName,
    string LastName
);