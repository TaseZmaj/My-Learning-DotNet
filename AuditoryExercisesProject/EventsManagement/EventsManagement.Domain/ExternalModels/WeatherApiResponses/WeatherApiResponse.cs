namespace EventsManagement.Domain.ExternalModels.WeatherApiResponses;
using System.Text.Json.Serialization;

public class WeatherApiResponse
{
    [JsonPropertyName("weather")]
    public List<WeatherCondition> Weather { get; set; } = [];

    [JsonPropertyName("main")]
    public WeatherMain Main { get; set; } = null!;

    [JsonPropertyName("wind")]
    public WeatherWind Wind { get; set; } = null!;

    [JsonPropertyName("visibility")]
    public int Visibility { get; set; }

    [JsonPropertyName("dt")]
    public long UnixTimestamp { get; set; }

    [JsonPropertyName("name")]
    public string CityName { get; set; } = string.Empty;
    
    [JsonPropertyName("condition")]
    public string Condition { get; set; } = string.Empty;
}