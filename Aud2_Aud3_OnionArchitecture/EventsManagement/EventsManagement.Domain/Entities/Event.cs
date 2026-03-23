using EventsManagement.Domain.Common;
using EventsManagement.Domain.Enums;

namespace EventsManagement.Domain.Entities;

public class Event : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? ImageUrl { get; set; }
    public EventStatus EventStatus;
    
    //MANY-TO-ONE
    public string? UserId { get; set; } //NOTE: NAMERNO e string UserId, bidejki nasleduva od IdentityUser EventsAppUser klasata
    public virtual EventsAppUser User { get; set; } = null!;
    
    //MANY-TO-ONE
    public Guid VenueId { get; set; }
    public virtual Venue Venue { get; set; }
    
    //ONE-TO-MANY
    public virtual ICollection<EventSectionPricing> SectionPricings { get; set; } = new List<EventSectionPricing>();
    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    public virtual ICollection<SeatReservation> SeatReservations { get; set; } = new List<SeatReservation>();
    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}