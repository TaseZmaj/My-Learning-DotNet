using EventsManagement.Domain.Common;
namespace EventsManagement.Domain.Entities;

public class Section : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    
    //MANY-TO-ONE
    public Guid VenueId { get; set; } //Foreign key
    public virtual Venue Venue { get; set; } = null!; //t.n. NAVIGATION PROPERTY koj pokazuva kon parent-ot
        //"virtual" vo kontekstot na Entity Framework se koristi za LAZY LOADING,
        //a obicno znaci deka neshto moze da e overridden

    // //ONE-TO-MANY
    // public virtual ICollection<Seat> Seats { get; set; } = new List<Seat>();
    //
    // //ONE-TO-MANY
    // public virtual ICollection<EventSectionPricing> EventSectionPricing { get; set; } =
    //     new List<EventSectionPricing>();
}