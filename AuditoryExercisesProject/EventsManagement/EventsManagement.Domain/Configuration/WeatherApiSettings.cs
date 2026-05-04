namespace EventsManagement.Domain.Configuration;

public class WeatherApiSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 30;
    public int CacheDurationMinutes { get; set; } = 120;
}