using Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Domain.Models;

public class ConsultationsApplicationUser : IdentityUser
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public Role Role { get; set; }
    
    public virtual ICollection<Attendance> Attendances { get; set; }
    public virtual ICollection<Holds> Holds { get; set; }
}