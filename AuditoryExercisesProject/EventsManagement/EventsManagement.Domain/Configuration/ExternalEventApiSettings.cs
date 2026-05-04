namespace EventsManagement.Domain.Configuration;

public class ExternalEventApiSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public string TokenUrl { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 30;

}