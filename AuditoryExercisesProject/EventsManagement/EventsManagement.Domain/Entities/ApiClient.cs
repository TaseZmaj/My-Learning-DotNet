using EventsManagement.Domain.Common;

namespace EventsManagement.Domain.Entities;

public class ApiClient : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int RateLimitPerMinute { get; set; } = 60;
    public DateTime CreatedAt { get; set; }
}