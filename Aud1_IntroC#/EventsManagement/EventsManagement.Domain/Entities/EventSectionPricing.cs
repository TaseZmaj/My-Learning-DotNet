using EventsManagement.Domain.Common;

namespace EventsManagement.Domain.Entities;

public class EventSectionPricing : BaseAuditableEntity<EventsAppUsers>
{
    public decimal Price { get; set; }
    public string Currency { get; set; }
    
    //MANY-TO-ONE
    public Guid EventId { get; set; }
    public virtual Event Event { get; set; } = null!;

    //MANY-TO-ONE
    public Guid SectionId { get; set; }
    public virtual Section Section { get; set; } = null!;
}