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

    public WeatherService(IEventService eventService, IWeatherApiClient weatherApiClient, ILogger<WeatherService> logger, IMemoryCache memoryCache, IOptions<WeatherApiSettings> weatherApiSettings)
    {
        _eventService = eventService;
        _weatherApiClient = weatherApiClient;
        _logger = logger;
        _memoryCache = memoryCache;
        _weatherApiSettings = weatherApiSettings.Value;
    }

    public async Task<EventWeatherDto> GetWeatherDataForEventIdAsync(Guid eventId)
    {
        // Find city and country
        var eventData = await _eventService.GetByIdAsync(eventId);

        var city = eventData.Venue.City;
        var country = eventData.Venue.Country;
        
        // Construct cache key
        var cacheKey = $"weather-api:{city}:{country}";

        // Check cache
        if (_memoryCache.TryGetValue(cacheKey, out EventWeatherDto? cached))
        {
            _logger.LogDebug(
                "Cache hit for event {EventId}", eventId);
            return cached;
        }
        
        // If present, return
        if (cached != null)
        {
            return cached;
        }

        // If not present, fetch
        var apiData = 
            await _weatherApiClient.GetWeatherForecastForCityAndCountry(city, country);

        // Put in the cache
        _memoryCache.Set(cacheKey, apiData, TimeSpan.FromMinutes(_weatherApiSettings.CacheDurationMinutes));
        
        _logger.LogInformation(
            "Weather cached for event {EventId}: " +
            "{Condition}, {Temp}°C",
            eventId, apiData.Condition, apiData.Temperature);
        
        return apiData;

    }
}