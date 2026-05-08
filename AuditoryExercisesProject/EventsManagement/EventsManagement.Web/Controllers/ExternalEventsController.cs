using EventsManagement.Domain.Dto;
using EventsManagement.Domain.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Service.Interfaces;

namespace EventsManagement.Web.Controllers;

[ApiController]
[Route("api/external/events")]
[EnableRateLimiting("external-api")]
public class ExternalEventsController : ControllerBase
{
    private readonly IInboundEventService  _inboundEventService;

    public ExternalEventsController(
        IInboundEventService inboundEventService)
    {
        _inboundEventService = inboundEventService;
    }
    
    [HttpPost]
    public async Task<IActionResult> ReceiveEvent(
        [FromBody] InboundEventRequest request, 
        CancellationToken ct)
    {
        var apiClient = HttpContext.Items["ApiClient"] as ApiClient;

        if (apiClient is null)
        {
            return Unauthorized();
        }
        
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest(new { error = "Title is required" });
        }

        if (request.StartDate >= request.EndDate)
        {
            return BadRequest(new { error = "StartDate must be before EndDate"});
        }
        
        var entry = await _inboundEventService.QueueAsync(request, apiClient);

        //Celava idea so status = "pending" e da ne ceka odgovor od APIto
        //onoj koj shto pratil request, zatoa dole imas metoda GetStatus za da moze
        //istiot da si proveri dali postoi vo baza kaj mene
        return Accepted(new
        {
            id = entry.Id,
            status = "pending",
            message = "Event queued for processing"
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetStatus(
        [FromBody] Guid id, CancellationToken ct)
    {
        var entry = _inboundEventService.GetStatusByIdAsync(id);

        if (entry is null)
        {
            return NotFound();
        }
        
        return Ok(new
        {
            id = entry.Id,
            status = entry.Status.ToString()
        });
    }
}