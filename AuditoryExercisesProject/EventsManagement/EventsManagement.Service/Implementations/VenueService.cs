using EventsManagement.Domain.Entities;
using EventsManagement.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Service.Interfaces;

namespace Service.Implementations;

public class VenueService : IVenueService
{
    private readonly IRepository<Venue> _venueRepository;

    public VenueService(IRepository<Venue> venueRepository)
    {
        _venueRepository = venueRepository;
    }

    public async Task<List<Venue>> GetAllAsync()
    {
        var result = await _venueRepository.GetAllAsync(x => x);
        return result.ToList();
    }

    public async Task<Venue?> GetByNameAndCityAsync(string venueName, string? city)
    {
        return await _venueRepository.GetAsync(
            selector: x => x,
            predicate: x => x.City == city && x.Name == venueName,
            include: x => x.Include(y => y.Sections
            ));
    }
    
    public async Task<Venue?> GetByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<Venue> GetByIdNotNullAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<Venue> InsertAsync(string name, string address, string city, string country, string? zipCode)
    {
        throw new NotImplementedException();
    }

    public async Task<Venue> UpdateAsync(Guid id, string name, string address, string city, string country, string? zipCode, int totalCapacity)
    {
        throw new NotImplementedException();
    }

    public async Task<Venue> DeleteAsync(Guid id)
    {
        throw new NotImplementedException();
    }
}