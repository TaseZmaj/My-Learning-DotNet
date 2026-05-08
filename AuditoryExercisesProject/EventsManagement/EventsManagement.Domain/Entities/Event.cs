using EventsManagement.Domain.Common;
using EventsManagement.Domain.Enums;

namespace EventsManagement.Domain.Entities;

public class Event : BaseAuditableEntity<EventsAppUser>
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? ImageUrl { get; set; }
    public EventStatus EventStatus { get; set; }
    
    //MANY-TO-ONE
    //FOR IMAGE UPLOADS
    public Guid? EventImageId { get; set; }
    public virtual EventsImages? EventImage { get; set; }
    
    //MANY-TO-ONE
    //NOTE: NAMERNO e string UserId, bidejki nasleduva od IdentityUser EventsAppUser klasata
    public string? UserId { get; set; } = null!;
    public virtual EventsAppUser User { get; set; } = null!;
    
    //MANY-TO-ONE
    public Guid VenueId { get; set; }
    public virtual Venue Venue { get; set; } = null!;
    
    //ONE-TO-MANY
    public virtual ICollection<EventSectionPricing> SectionPricings { get; set; } = new List<EventSectionPricing>();
    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    public virtual ICollection<SeatReservation> SeatReservations { get; set; } = new List<SeatReservation>();
    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}