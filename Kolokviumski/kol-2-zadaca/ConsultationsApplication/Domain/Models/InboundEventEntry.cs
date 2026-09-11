using Domain.Common;
using Domain.Enums;

namespace Domain.Models;

public class InboundEventEntry : BaseEntity
{
    public string RawPayload { get; set; } = string.Empty;
    public InboundEventStatus Status { get; set; }
    public DateTime ReceivedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public Guid? AttendanceId { get; set; }
}