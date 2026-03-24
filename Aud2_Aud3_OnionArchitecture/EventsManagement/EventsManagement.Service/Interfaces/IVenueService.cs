using EventsManagement.Domain.Entities;

namespace Service.Interfaces;

public interface IVenueService
{
    public Task<List<Venue>> GetAllAsync();
    public Task<Venue?> GetByIdAsync(Guid id);
    public Task<Venue> GetByIdNotNullAsync(Guid id);

    public Task<Venue> InsertAsync(string name, string address, string city, string country, string? zipCode);
    public Task<Venue> UpdateAsync(Guid id, string name, string address, string city, string country, string? zipCode, int totalCapacity);
    public Task<Venue> DeleteAsync(Guid id);
}