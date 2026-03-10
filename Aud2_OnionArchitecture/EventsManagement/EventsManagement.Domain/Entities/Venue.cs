using EventsManagement.Domain.Common;

namespace EventsManagement.Domain.Entities;

public class Venue : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public int ZipCode { get; set; }
    public int TotalCapacity { get; set; }
    
    //ONE-TO-MANY
    public virtual ICollection<Section> Sections { get; set; } = new List<Section>();
    
    //ONE-TO-MANY
    public virtual ICollection<Event>? Events { get; set; } = new List<Event>();
}