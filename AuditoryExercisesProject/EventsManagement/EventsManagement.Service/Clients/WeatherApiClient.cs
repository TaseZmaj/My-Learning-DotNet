using System.Net.Http.Json;
using EventsManagement.Domain.Configuration;
using EventsManagement.Domain.Dto;
using EventsManagement.Domain.ExternalModels.WeatherApiResponses;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Service.Interfaces;

namespace Service.Clients;

public class WeatherApiClient : IWeatherApiClient
{
    private readonly ILogger<WeatherApiClient> _logger;
    private readonly HttpClient _http;
    private readonly WeatherApiSettings _settings;

    public WeatherApiClient(
        HttpClient http, 
        IOptions<WeatherApiSettings> options, 
        ILogger<WeatherApiClient> logger)
    {
        _http = http;
        _settings = options.Value;
        _logger = logger;
    }
    
    public async Task<WeatherApiResponse?> GetWeatherForecastForCityAndCountry(
        string city, string country, CancellationToken ct)
    {
        var url = $"forecast?q={city},country={country}" +
                  $"&appid={_settings.ApiKey}&units=metric";
        
        _logger.LogInformation($"Fetching forecast for city:{city} and country: {country}");

        var response = await _http.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<WeatherApiResponse>(ct);
    }

    public async Task<EventWeatherDto?> GetCurrentWeatherAsync(double lat, double lon, CancellationToken ct)
    {
        var url = $"weather?lat={lat}&lon={lon}" +
                  $"&appid={_settings.ApiKey}&units=metric";

        _logger.LogInformation(
            "Calling weather API for lat={Lat}, lon={Lon}", 
            lat, lon);

        var response = await _http.GetAsync(url, ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "Weather API returned {Status}", 
                (int)response.StatusCode);
            return null;
        }

        var data = await response.Content
            .ReadFromJsonAsync<WeatherApiResponse>(ct);

        if (data is null) return null;

        // TRANSFORM - API response to our DTO
        return new EventWeatherDto
        {
            Temperature = Math.Round(data.Main.Temperature, 1),
            FeelsLike = Math.Round(data.Main.FeelsLike, 1),
            TempMin = Math.Round(data.Main.TempMin, 1),
            TempMax = Math.Round(data.Main.TempMax, 1),
            Humidity = data.Main.Humidity,
            WindSpeed = Math.Round(data.Wind.Speed, 1),
            Condition = data.Weather
                .FirstOrDefault()?.Main ?? "Unknown",
            Description = data.Weather
                .FirstOrDefault()?.Description ?? "",
            Icon = data.Weather
                .FirstOrDefault()?.Icon ?? "",
            RetrievedAt = DateTime.UtcNow
        };
    }
}