using EventsManagement.Domain.Common;
using EventsManagement.Domain.Enums;

namespace EventsManagement.Domain.Entities;

public class Event : BaseEntity
{
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string BannerUrl { get; set; }
    public Status Status;
    
    //MANY-TO-ONE
    public Guid UserId { get; set; }
    public virtual ApplicationUser ApplicationUser { get; set; }
    
    //MANY-TO-ONE
    public Guid VenueId { get; set; }
    public virtual Venue Venue { get; set; }
}