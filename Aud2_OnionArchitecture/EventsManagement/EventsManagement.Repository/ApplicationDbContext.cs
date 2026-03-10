using EventsManagement.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EventsManagement.Repository;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Event> Events { get; set; }
    public DbSet<EventSectionPricing> EventSectionPricings { get; set; }
    public DbSet<Seat> Seats { get; set; }
    public DbSet<Section> Sections { get; set; }
    public DbSet<Venue> Venues { get; set; }
}