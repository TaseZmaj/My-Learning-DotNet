using Microsoft.AspNetCore.Identity;

namespace EventsManagement.Domain.Entities;

//vaka se pravi ako sakash da imas custom User klasa vo proektot, NO
//.NET Core vekje si ima svoi useri, a toa e IdentityUser.cs
public class EventsAppUser : IdentityUser
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
}