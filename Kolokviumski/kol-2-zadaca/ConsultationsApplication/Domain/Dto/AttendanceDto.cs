using Domain.Enums;
using Domain.Models;

namespace Domain.Dto;

public class AttendanceDto
{
    public string? Comment { get; set; }
    public required string UserId { get; set; }
    public Guid RoomId { get; set; }
    public Guid ConsultationId { get; set; }
}