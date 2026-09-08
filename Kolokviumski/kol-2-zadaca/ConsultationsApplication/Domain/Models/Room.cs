using Domain.Common;

namespace Domain.Models;

public class Room : BaseEntity
{
    public string Name { get; set; }
    public int Capacity { get; set; }
    public virtual ICollection<Attendance> Attendances { get; set; }
}