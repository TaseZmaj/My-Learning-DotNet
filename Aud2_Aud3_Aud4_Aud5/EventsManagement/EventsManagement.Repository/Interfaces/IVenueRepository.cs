using EventsManagement.Domain.Entities;

namespace EventsManagement.Repository.Interfaces;

public interface IVenueRepository
{
    Task BulkInsertOrUpdateVenuesAsync(List<Venue> venues);
    Task BulkInsertOrUpdateSectionsAsync(List<Section> sections);
    Task BulkInsertOrUpdateSeatsAsync(List<Seat> seats);
}