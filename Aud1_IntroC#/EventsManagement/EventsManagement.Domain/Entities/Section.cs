using EventsManagement.Domain.Common;
namespace EventsManagement.Domain.Entities;

public class Section : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    
    
    //BITNO: Spored ovie 2 tuka VenueId i Venue - Entity Framework doagja do
      //zaklucok deka stanuva zbor za Foreign Key
      //Ovoj relationship shto e definiran tuka e Many-To-One
      //(1 section moze da pripagja samo na 1 venue T.E. 1 Venue - N sections)
    
    //MANY-TO-ONE
    public Guid VenueId { get; set; } //Foreign key
    public virtual Venue venue { get; set; } = null!; //t.n. NAVIGATION PROPERTY koj pokazuva kon parent-ot
        //"virtual" vo kontekstot na Entity Framework se koristi za LAZY LOADING,
        //a obicno znaci deka neshto moze da e overridden

    //ONE-TO-MANY
    public virtual ICollection<Seat> Seats { get; set; } = new List<Seat>();

    //ONE-TO-MANY
    public virtual ICollection<EventSectionPricing> EventSectionPricing { get; set; } =
        new List<EventSectionPricing>();
}