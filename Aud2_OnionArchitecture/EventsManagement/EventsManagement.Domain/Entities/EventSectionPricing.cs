using EventsManagement.Domain.Common;

namespace EventsManagement.Domain.Entities;

public class EventSectionPricing : BaseAuditableEntity<ApplicationUser>
{
    public decimal Price { get; set; }
    public string Currency { get; set; }
    
    //MANY-TO-ONE
    public Guid EventId { get; set; }
    public virtual required Event Event { get; set; } 

    //MANY-TO-ONE
    public Guid SectionId { get; set; }
    public virtual required Section Section { get; set; } 
}