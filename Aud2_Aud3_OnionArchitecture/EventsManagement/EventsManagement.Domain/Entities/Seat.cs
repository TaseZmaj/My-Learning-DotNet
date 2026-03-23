using EventsManagement.Domain.Common;

namespace EventsManagement.Domain.Entities;

public class Seat : BaseEntity
{
    public int Row { get; set; }
    public int Number { get; set; }
    public string? Label { get; set; } = string.Empty;
    public bool IsAccessible { get; set; }
    
    //MANY-TO-ONE
    public Guid SectionId { get; set; } //Foreign key
    public virtual Section Section { get; set; } = null!; //Navigation property
    
    //ONE-TO-MANY
    public virtual ICollection<SeatReservation> SeatReservations { get; set; } = new List<SeatReservation>();
}