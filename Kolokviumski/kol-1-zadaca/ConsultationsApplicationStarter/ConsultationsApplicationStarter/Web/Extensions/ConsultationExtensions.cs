using Domain.Models;
using Web.Response;

namespace Web.Extensions;

public static class ConsultationExtensions
{
    
    public static BasicConsultationResponse ToBasicResponse(this Consultation c)
    {
        return new BasicConsultationResponse(
            c.Id,
            c.RoomId,
            c.StartTime,
            c.EndTime
        );
    }
    
    public static ConsultationResponse ToResponse(this Consultation c)
    {
        return new ConsultationResponse(
            c.Id,
            DateOnly.FromDateTime(c.StartTime),
            c.RoomId,
            // c.Room.Name,
            c.RegisteredStudents,
            c.Attendances.Select(x => x.ToBasicResponse()).ToList()
        );
    }

    public static List<ConsultationResponse> ToResponse(this List<Consultation> consultations)
    {
        return consultations.Select(x => x.ToResponse()).ToList();
    }
}