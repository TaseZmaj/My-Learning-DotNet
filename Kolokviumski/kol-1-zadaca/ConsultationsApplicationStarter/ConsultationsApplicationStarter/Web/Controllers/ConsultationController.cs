using Microsoft.AspNetCore.Mvc;
using Web.Mapper;
using Web.Request;
using Web.Response;

namespace Web.Controllers;

[Route("/api/Consultation")]
[ApiController]
public class ConsultationController : ControllerBase
{
    public readonly ConsultationMapper _consultationMapper;
    
    
    public ConsultationController(ConsultationMapper consultationMapper)
    {
        _consultationMapper = consultationMapper;
    }
    
    [HttpGet]
    public async Task<List<ConsultationResponse>> GetAll(
        [FromQuery] string? roomName, 
        [FromQuery] DateOnly? date)
    {
        return await _consultationMapper.GetAllAsync(roomName, date);
    }
    
    [HttpPost]
    public async Task<IActionResult> Insert([FromBody] ConsultationRequest request)
    {
        var result = await _consultationMapper.InsertAsync(request);
        return Ok(result);
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] ConsultationRequest consultationRequest)
    {
        var result = await _consultationMapper.UpdateAsync(id, consultationRequest);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        var result = await _consultationMapper.DeleteAsync(id);
        return Ok(result); 
    }
    
    [HttpGet("paged")]
    public async Task<PaginatedResponse<ConsultationResponse>> Paged([FromQuery] PaginatedRequest request)
    {
        return await _consultationMapper.PaginatedGetAllAsync(request);
    }
}