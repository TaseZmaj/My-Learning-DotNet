using EventsManagement.Domain.Common;

namespace EventsManagement.Domain.Entities;

public class Seat : BaseEntity
{
    public int RowNumber { get; set; }
    public int SeatNumber { get; set; }
    public string Label { get; set; } = string.Empty;
    public bool IsAccessible { get; set; }
    
    //MANY-TO-ONE
    public Guid SectionId { get; set; } //Foreign key
    public virtual Section Section { get; set; } = null!; //Navitagion property
    
    
}