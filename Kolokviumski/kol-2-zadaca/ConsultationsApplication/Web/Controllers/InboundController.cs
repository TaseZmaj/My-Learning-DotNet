using System.Text.Json;
using Domain.Dto;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Service.Interface;
namespace Web.Controllers;

[ApiController]
[Route("api/external/attendance")]
public class InboundController : ControllerBase
{
    private readonly IInboundEventEntryService _inboundEventEntryService;

    public InboundController(IInboundEventEntryService inboundEventEntryService)
    {
        _inboundEventEntryService = inboundEventEntryService;
    }

    [HttpPost("register")]
    [EnableRateLimiting("external-api")]
    public async Task<IActionResult> Register([FromBody] InboundEventRequest request)
    {
        // var apiClient = HttpContext.Items["ApiClient"] as ApiClient;
        //
        // if (apiClient is null)
        // {
        //     return Unauthorized();
        // }
        
        var payload = JsonSerializer.Serialize(request);

        var entry = await _inboundEventEntryService.CreateAsync(payload);
        
        return Accepted(new
        {
            id = entry.Id,
            status = "pending",
        });
    }
    
    [HttpGet("register/{id}/status")]
    [EnableRateLimiting("external-api")]
    public async Task<IActionResult> GetStatus(Guid id, CancellationToken ct)
    {
        var entry = await _inboundEventEntryService.GetVyIdNotNullAsync(id);

        // if (entry is null)
        // {
        //     return NotFound();
        // }
        return Ok(new
        {
            id = entry.Id,
            status = entry.Status.ToString().ToLower(),
            Error = entry.ErrorMessage,
        });
    }
}