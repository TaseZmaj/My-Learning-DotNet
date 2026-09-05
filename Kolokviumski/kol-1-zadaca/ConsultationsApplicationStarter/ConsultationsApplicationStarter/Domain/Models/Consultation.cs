using Domain.Common;

namespace Domain.Models;

public class Consultation : BaseAuditableEntity<ConsultationsApplicationUser>
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public Guid RoomId { get; set; }
    public virtual Room Room { get; set; }
    
    public int RegisteredStudents { get; set; }
    
    public virtual ICollection<Attendance> Attendances { get; set; }
    public virtual ICollection<Holds> Holds { get; set; }
}