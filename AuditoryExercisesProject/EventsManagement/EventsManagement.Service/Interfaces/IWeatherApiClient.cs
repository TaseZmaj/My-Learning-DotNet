using EventsManagement.Domain.Dto;
using EventsManagement.Domain.ExternalModels.WeatherApiResponses;

namespace Service.Interfaces;

public interface IWeatherApiClient
{
    public Task<EventWeatherDto?> GetWeatherForecastForCityAndCountry(string city, string country);
    public Task<EventWeatherDto?> GetCurrentWeatherAsync(double lat, double lon, CancellationToken ct);
}