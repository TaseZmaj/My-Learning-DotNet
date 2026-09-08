using Microsoft.AspNetCore.Http;
using Service.Interface;

namespace Service.Implementation;

using System.Security.Claims;


public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _accessor;

    public CurrentUserService(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    public string? GetUserId()
    {
        return _accessor?.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}