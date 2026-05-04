using EventsManagement.Domain.ExternalModels;

namespace Service.Interfaces;

public interface IExternalEventApiClient
{
    public Task<List<ExternalEventDto>> GetEventsAsync(CancellationToken ct);
}