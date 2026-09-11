namespace Domain.Models;

public class ApiClient
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int RateLimitMinutes { get; set; }
}