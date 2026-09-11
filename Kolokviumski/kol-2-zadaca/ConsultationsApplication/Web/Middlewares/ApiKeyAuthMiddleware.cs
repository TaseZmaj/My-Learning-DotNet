using Domain.Configuration;
using Microsoft.Extensions.Options;
using Repository;

namespace Web.Middlewares;

public class ApiKeyAuthMiddleware
{
    private readonly RequestDelegate _next;

    public ApiKeyAuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context, 
        ApplicationDbContext dbContext,
        IOptions<ApiKeySettings> apiKeySettings
        )
    {
        if (!context.Request.Path.StartsWithSegments("/api/external"))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue("X-Api-Key", out var key))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new
            {
                Error = "Api Key is Required"
            });
            return;
        }
        
        if (key != apiKeySettings.Value.ApiKey)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Invalid API key");
            return;
        }

        await _next(context);
    }
}