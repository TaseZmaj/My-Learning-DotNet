using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Query;
using Service.Implementation;
using Service.Interface;
using Web.Mapper;
using Web.Request;
using Web.Response;

namespace Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConsultationController : ControllerBase
{
    
    private readonly ConsultationMapper _consultationMapper;
    private readonly IConsultationService _consultationService;

    public ConsultationController(ConsultationMapper consultationMapper, IConsultationService consultationService)
    {
        _consultationMapper = consultationMapper;
        _consultationService = consultationService;
    }

    [HttpGet("")]
    public async Task<List<ConsultationResponse>> GetAllAsync([FromQuery] string? roomName, [FromQuery] DateOnly? date)
    {
        return await _consultationMapper.GetAllAsync(roomName, date);
    }
    
    [HttpGet("paged")]
    public async Task<PaginatedResponse<ConsultationResponse>> GetAllPaged([FromQuery] PaginatedRequest request)
    {
        return await _consultationMapper.GetAllPaginatedAsync(request);
    }

    [HttpPost]
    public async Task<IActionResult> RegisterAsync([FromBody] ConsultationRequest request)
    {
        var result =  await _consultationMapper.InsertAsync(request);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAsync([FromRoute] Guid id, [FromBody] ConsultationRequest request)
    {
        var result = await _consultationMapper.UpdateAsync(id, request);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync([FromRoute] Guid id)
    {
        var result = await _consultationMapper.DeleteAsync(id);
        return Ok(result);
    }
}