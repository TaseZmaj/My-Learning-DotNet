using Domain.Models;
using Web.Response;

namespace Web.Extensions;

public static class AttendanceExtensions
{
    public static BasicAttendanceResponse ToBasicResponse(this Attendance attendance)
    {
        return new BasicAttendanceResponse(
            attendance.Id,
            attendance.User.FirstName,
            attendance.User.LastName
        );
    }
}