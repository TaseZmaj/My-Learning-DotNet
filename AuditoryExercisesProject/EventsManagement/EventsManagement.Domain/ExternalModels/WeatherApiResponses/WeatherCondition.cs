using System.Text.Json.Serialization;

namespace EventsManagement.Domain.ExternalModels.WeatherApiResponses;

public class WeatherCondition
{
    [JsonPropertyName("main")]
    public string Main { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("icon")]
    public string Icon { get; set; } = string.Empty;
}