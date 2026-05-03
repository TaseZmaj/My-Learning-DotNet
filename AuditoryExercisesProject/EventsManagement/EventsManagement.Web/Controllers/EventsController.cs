using EventsManagement.Web.Mapper;
using EventsManagement.Web.Response;
using EventsManagement.Web.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventsManagement.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class EventsController : ControllerBase
{
    private readonly EventMapper _eventMapper;
    
    public EventsController(EventMapper eventMapper)
    {
        _eventMapper = eventMapper;
    }

    [HttpGet]
    public async Task<List<EventResponse>> GetAll()
    {
        return await _eventMapper.GetAll();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        var result = await _eventMapper.GetById(id);
        if (result == null)
        {
            return NotFound();
        }
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Insert([FromBody] EventRequest eventRequest)
    {
        var result = await _eventMapper.InsertAsync(eventRequest);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] EventRequest eventRequest)
    {
        var result = await _eventMapper.UpdateAsync(id, eventRequest);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        var result = await _eventMapper.DeleteAsync(id);
        return Ok(result);   
    }

    [HttpGet("paged")]
    public async Task<PaginatedResponse<EventResponse>> Paged([FromQuery] PaginateRequest request)
    {
        return await _eventMapper.PaginatedGetAllAsync(request);
    }

    [HttpPost("upload-image/{eventId}")]
    public async Task<IActionResult> UploadImageByIdAsync([FromRoute] Guid eventId, [FromForm] IFormFile file)
    {
        var result = await _eventMapper.UploadImageByIdAsync(eventId, file);
        return Ok(result);
    }

    [HttpPost("upload-image-fs/{eventId}")]
    public async Task<IActionResult> UploadImageByIdInFileSystemAsync([FromRoute] Guid eventId,
        [FromForm] IFormFile file)
    {
        var result = await _eventMapper.UploadImageByIdInFileSystemAsync(eventId, file);
        return Ok(result);
    }
}