using System.Security.Claims;
using Service.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Service.Implementations;

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _accessor;

    public CurrentUser(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }
    
    //Everything is nullable (string?) — because there might not be an authenticated user (anonymous requests)
    //t.e. koga nekoj pravi request ali ne e logiran
    public string? GetUserId()
    {
        return _accessor?.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}