using EventsManagement.Domain.Common;
using EventsManagement.Domain.Enums;

namespace EventsManagement.Domain.Entities;

public class InboundEventEntry : BaseEntity
{
    public string RawPayload { get; set; } = string.Empty;
    public InboundEventStatus Status { get; set; }
    public Guid ApiClientId { get; set; }
    public virtual ApiClient ApiClient { get; set; } = null!;
    public DateTime ReceivedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public Guid? CreatedEventId { get; set; }
}