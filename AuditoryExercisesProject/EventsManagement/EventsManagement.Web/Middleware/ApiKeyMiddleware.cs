using EventsManagement.Repository;

namespace EventsManagement.Web.Middleware;

public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private const string API_KEY_HEADER = "X-API-Key";

    public ApiKeyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ApplicationDbContext db)
    {
        if (!context.Request.Path.StartsWithSegments("/api/external"))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(API_KEY_HEADER, out var apiKey))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Api key is required"
            });
            return;
        }
        
        var client = db.ApiClients.FirstOrDefault(c => c.ApiKey == apiKey.ToString() && c.IsActive);

        if (client is null)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Invalid or inactive API key"
            });
            return;
        }

        context.Items["ApiClient"] = client;
        await _next(context);
    }
}