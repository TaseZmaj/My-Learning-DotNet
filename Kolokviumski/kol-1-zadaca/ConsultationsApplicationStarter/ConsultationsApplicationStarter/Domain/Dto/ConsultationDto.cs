namespace Domain.Dto;

public class ConsultationDto
{
    public Guid RoomId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    
}