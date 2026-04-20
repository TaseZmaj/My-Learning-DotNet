using EFCore.BulkExtensions;
using EventsManagement.Domain.Entities;
using EventsManagement.Repository.Interfaces;

namespace EventsManagement.Repository.Implementations;

public class VenueRepository : Repository<Venue>, IVenueRepository
{
    public VenueRepository(ApplicationDbContext context) : base(context)
    {
    }
    
    public async Task BulkInsertOrUpdateVenuesAsync(List<Venue> venues)
    {
        await _context.BulkInsertOrUpdateAsync(venues);
    }

    public async Task BulkInsertOrUpdateSectionsAsync(List<Section> sections)
    {
        await _context.BulkInsertOrUpdateAsync(sections);
    }

    public async Task BulkInsertOrUpdateSeatsAsync(List<Seat> seats)
    {
        await _context.BulkInsertOrUpdateAsync(seats);
    }
}