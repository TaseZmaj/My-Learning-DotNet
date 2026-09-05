namespace Web.Response;

public record BasicAttendanceResponse(
    Guid Id,
    string FirstName,
    string LastName
);