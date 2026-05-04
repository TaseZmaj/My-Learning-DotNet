using EventsManagement.Domain.Configuration;
using EventsManagement.Domain.Dto;
using Service.Clients;
using Service.Interfaces;

namespace Service.Implementations;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class WeatherService : IWeatherService
{
    private readonly IEventService _eventService;
    private readonly IWeatherApiClient _weatherApiClient;
    private readonly ILogger<WeatherService> _logger;
    private readonly IMemoryCache _memoryCache;
    private readonly WeatherApiSettings _weatherApiSettings;

    public WeatherService(
        IEventService eventService,
        IWeatherApiClient weatherApiClient,
        ILogger<WeatherService> logger,
        IMemoryCache memoryCache,
        IOptions<WeatherApiSettings> weatherApiSettings
    )
    {
        _eventService = eventService;
        _weatherApiClient = weatherApiClient;
        _logger = logger;
        _memoryCache = memoryCache;
        _weatherApiSettings = weatherApiSettings.Value;
    }

    public async Task<EventWeatherDto?> GetWeatherDataForEventIdAsync(Guid eventId)
    {
        // Find city and country from the existing event data
        var eventData = await _eventService.GetByIdAsync(eventId);
        
        var city = eventData.Venue.City;
        var country = eventData.Venue.Country;

        // Construct a unique cache key based on the location
        var cacheKey = $"weather-api:{city}:{country}";

        // Check if the data is already in the cache
        if (_memoryCache.TryGetValue(cacheKey, out EventWeatherDto? cached))
        {
            _logger.LogDebug("Cache hit for event {EventId}", eventId);
            return cached;
        }

        // If not present, fetch from the external API
        var apiData = await _weatherApiClient.GetWeatherForecastForCityAndCountry(city, country);

        if (apiData == null) return null;

        // Put the result in the cache using the expiration time from settings
        _memoryCache.Set(cacheKey, apiData, 
            TimeSpan.FromMinutes(_weatherApiSettings.CacheDurationMinutes));

        _logger.LogInformation(
            $"Weather cached for event {EventId}: {Condition}, {Temp}°C", eventId);
        
        return apiData;
    }
}