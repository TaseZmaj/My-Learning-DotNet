using EventsManagement.Domain.Common;
using EventsManagement.Domain.Enums;

namespace EventsManagement.Domain.Entities;

public class Reservation : BaseAuditableEntity<EventsAppUser>
{
    public DateTime ReservedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public ReservationStatus Status { get; set; }

    //MANY-TO-ONE
    public Guid EventId { get; set; }
    public virtual Event Event { get; set; } = null!;
    
    //MANY-TO-ONE
    public string UserId { get; set; } = null!;
    public virtual EventsAppUser User { get; set; } = null!;
    
    //ONE-TO-MANY
    public virtual ICollection<SeatReservation> SeatReservations { get; set; } = new List<SeatReservation>();
}