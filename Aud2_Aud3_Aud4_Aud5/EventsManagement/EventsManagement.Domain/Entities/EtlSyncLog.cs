using EventsManagement.Domain.Common;

namespace EventsManagement.Domain.Entities;

public class EtlSyncLog : BaseEntity
{
    public string JobName { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}