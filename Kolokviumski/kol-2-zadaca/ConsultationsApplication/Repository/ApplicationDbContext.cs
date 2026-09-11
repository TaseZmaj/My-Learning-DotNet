using Domain.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Repository;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ConsultationsApplicationUser>(options)
{
    public DbSet<Consultation> Consultations { get; set; }
    public DbSet<Attendance> Attendances { get; set; }
    public DbSet<Holds> Holds { get; set; }
    public DbSet<Room> Rooms { get; set; }
    
    public DbSet<InboundEventEntry> InboundEventEntries { get; set; }
    public DbSet<ApiClient> ApiClients { get; set; }
    public DbSet<EtlSyncLog> EtlSyncLogs { get; set; }
}