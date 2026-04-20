using EventsManagement.Domain.Common;

namespace EventsManagement.Domain.Entities;

public class SeatReservation : BaseEntity
{
    public Guid EventId { get; set; }
    public virtual Event Event { get; set; } = null!;
    
    public Guid SeatId { get; set; }
    public virtual Seat Seat { get; set; } = null!;
    
    public Guid ReservationId { get; set; }
    public virtual Reservation Reservation { get; set; } = null!;

    public virtual Ticket? Ticket { get; set; }
}