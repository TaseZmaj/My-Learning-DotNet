using EventsManagement.Domain.Dto;

namespace Service.Interfaces;

public interface IWeatherService
{
    Task<EventWeatherDto> GetWeatherDataForEventIdAsync(Guid eventId);
}