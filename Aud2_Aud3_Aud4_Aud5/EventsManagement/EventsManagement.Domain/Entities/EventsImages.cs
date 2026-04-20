using EventsManagement.Domain.Common;

namespace EventsManagement.Domain.Entities;

public class EventsImages : BaseEntity
{
    public string FileName { get; set; }
    public string ContentType { get; set; }
    public byte[] Data { get; set; }
    public long Size { get; set; }
    
    //Foreign key to the main entity
    public virtual Event Event { get; set; }
}