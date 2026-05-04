using EventsManagement.Domain.Dto;
using EventsManagement.Domain.ExternalModels.WeatherApiResponses;

namespace Service.Interfaces;

public interface IWeatherApiClient
{
    public Task<WeatherApiResponse?> GetWeatherForecastForCityAndCountry(string city, string country, CancellationToken ct);
    public Task<EventWeatherDto?> GetCurrentWeatherAsync(double lat, double lon, CancellationToken ct);
}