namespace EventsManagement.Domain.ExternalModels.WeatherApiResponses;
using System.Text.Json.Serialization;

public class WeatherWind
{
    [JsonPropertyName("speed")]
    public double Speed { get; set; }

    [JsonPropertyName("deg")]
    public int Degrees { get; set; }
}