using Domain.Common;
using Domain.Enums;

namespace Domain.Models;

public class Attendance : BaseEntity
{
    public string? Comment { get; set; }
    public Status Status { get; set; }
    
    public required string UserId { get; set; }
    public virtual ConsultationsApplicationUser User { get; set; } = null!;
    
    public Guid RoomId { get; set; }
    public virtual Room Room { get; set; } = null!;
    
    public Guid ConsultationId { get; set; }
    public virtual Consultation Consultation { get; set; } = null!;
    
    public string? CancellationReasonDocumentPath { get; set; }
}