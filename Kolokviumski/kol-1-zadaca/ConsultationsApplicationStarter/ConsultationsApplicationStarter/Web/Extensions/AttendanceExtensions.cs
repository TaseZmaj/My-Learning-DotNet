using Domain.Dto;
using Domain.Enums;
using Domain.Models;
using Web.Request;
using Web.Response;

namespace Web.Extensions;

public static class AttendanceExtensions
{
    public static AttendanceDto ToDto(this AttendanceRequest attendance)
    {
        return new AttendanceDto
        {
            Comment = attendance.Comment,
            UserId = attendance.UserId,
            RoomId = attendance.RoomId,
            ConsultationId =  attendance.ConsultationId
        };
    }
    public static BasicAttendanceResponse ToBasicResponse(this Attendance attendance)
    {
        return new BasicAttendanceResponse(
            attendance.Id,
            attendance.User.FirstName,
            attendance.User.LastName
        );
    }

    public static AttendanceResponse ToResponse(this Attendance attendance)
    {
        return new AttendanceResponse(
            attendance.Id,
            attendance.UserId,
            attendance.User.FirstName,
            attendance.User.LastName,
            attendance.Status,
            attendance.Comment
        );
    }
    
    public static List<AttendanceResponse> ToResponse(this List<Attendance> reservations)
    {
        return reservations.Select(x => x.ToResponse()).ToList();
    }
}