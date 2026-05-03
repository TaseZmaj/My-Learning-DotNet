using EventsManagement.Domain.Entities;
using EventsManagement.Domain.ExternalModels;
using EventsManagement.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventsManagement.Repository.Implementations;

public class LegacyVenueRepository : ILegacyVenueRepository
{
    private readonly LegacyApplicationDbContext _dbContext;

    public LegacyVenueRepository(LegacyApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    
    public async Task<List<Venue>> GetVenuesModifiedSinceAsync(DateTime since)
    {
        var legacy = await _dbContext.Venues.Where(x => x.LastModified >= since).ToListAsync();

        //vo returnot go pravis mapiranjeto od LegacyVenue entitet vo Venue entitet
        return legacy.Select(x => new Venue()
        {
            Id = GuidHelper.FromLegacyId("Venue", x.VenueId),
            Name = x.Name,
            Address = x.Address,
            City = x.City,
            Country = x.Country,
            TotalCapacity = x.TotalCapacity,
            ZipCode = x.ZipCode,
        }).ToList();
    }

    public async Task<List<Section>> GetSectionsModifiedSinceAsync(DateTime since)
    {
        var legacySections = await _dbContext.Sections
            .AsNoTracking()
            .Where(s => s.LastModified > since)
            .ToListAsync();

        return legacySections.Select(ls => new Section
        {
            Id = GuidHelper.FromLegacyId("Section", ls.SectionId),
            VenueId = GuidHelper.FromLegacyId("Venue", ls.VenueId),
            Name = ls.Name.Trim(),
            Capacity = ls.Capacity
        }).ToList();
    }

    public async Task<List<Seat>> GetSeatsModifiedSinceAsync(DateTime since)
    {
        var legacySeats = await _dbContext.Seats
            .AsNoTracking()
            .Where(s => s.LastModified > since)
            .ToListAsync();

        return legacySeats.Select(ls => new Seat
        {
            Id = GuidHelper.FromLegacyId("Seat", ls.SeatId),
            SectionId = GuidHelper.FromLegacyId("Section", ls.SectionId),
            Row = ls.Row,
            Number = ls.Number,
            Label = "",
            IsAccessible = ls.IsAccessible
        }).ToList();
    }
}