using Microsoft.AspNetCore.Identity;

namespace EventsManagement.Domain.Entities;

//vaka se pravi ako sakash da imas custom User klasa vo proektot, NO
//.NET Core vekje si ima svoi useri, a toa e IdentityUser.cs
public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    
    //One-To-Many
    public virtual ICollection<Event> Events { get; set; } = new List<Event>();
}