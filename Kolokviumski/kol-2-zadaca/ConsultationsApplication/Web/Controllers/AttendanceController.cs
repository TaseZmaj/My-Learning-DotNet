using Microsoft.AspNetCore.Mvc;
using Web.Mapper;
using Web.Request;

namespace Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AttendanceController : ControllerBase
{
    private readonly AttendanceMapper _attendanceMapper;

    public AttendanceController(AttendanceMapper attendanceMapper)
    {
        _attendanceMapper = attendanceMapper;
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterAsync([FromBody] AttendanceRequest request)
    {
        var result = await _attendanceMapper.RegisterAsync(request);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        await _attendanceMapper.DeleteAsync(id);
        return Ok();
    }

    [HttpGet("consultation/{id}")]
    public async Task<IActionResult> GetConsultationAsync(Guid id)
    {
        var result = await _attendanceMapper.GetAllByConsultationIdAsync(id);
        return Ok(result);
    }

    [HttpPatch("{id}/mark-as-absent")]
    public async Task<IActionResult> MarkAsAbsentAsync(Guid id)
    {
        await  _attendanceMapper.MarkAsAbsentAsync(id);
        return Ok();
    }
    
    [HttpPost("{id}/cancelation-reason")]
    public async Task<IActionResult> UploadImageByIdInFileSystemAsync([FromRoute] Guid id, [FromForm] IFormFile file)
    {
        var result = await _attendanceMapper.UploadReasonByIdInFileSystemAsync(id, file);
        return Ok(result);
    }
}