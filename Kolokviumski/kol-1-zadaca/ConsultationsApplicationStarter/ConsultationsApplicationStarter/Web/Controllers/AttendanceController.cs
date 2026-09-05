using Microsoft.AspNetCore.Mvc;
using Web.Mapper;
using Web.Request;

namespace Web.Controllers;

[ApiController]
[Route("api/Attendance")]
public class AttendanceController : ControllerBase
{
    private readonly AttendanceMapper _attendanceMapper;

    public AttendanceController(AttendanceMapper attendanceMapper)
    {
        _attendanceMapper = attendanceMapper;
    }
    
    
    //POST /api/attendance/register — Пријавување на термин за консултации
    // Параметри кои се испраќаат во телото на барањето:
    //
    // ConsultationId
    //     UserId
    // RoomId
    //     Comment?
    // При успешно извршување на трансакција:
    //
    // Потребно е да се зголеми бројот на пријавени студенти во соодветниот термин за консултации
    // Атрибутот Status при регистрација се поставува на Registered
    // Враќа:
    //
    // Id
    //     UserId
    // FirstName
    //     LastName
    // Status - како текстуална вредност
    //     Comment?
    
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] AttendanceRequest attendanceRequest)
    {
        var result = await _attendanceMapper.RegisterAsync(attendanceRequest);
        return Ok(result);
    }

    
    // DELETE /api/attendance/{id} — Бришење на присуство
    //     Напомена:
    //
    // Не е дозволено бришење на пријава за термин за консултации чиј
    // StartTime е помалку од 1 час во иднина.Потребно е да се намали
    // бројот на пријавени студенти во терминто за консултации. При
    // успешно извршување враќа статус 200

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute]Guid id)
    {
        var result = await _attendanceMapper.DeleteAsync(id);
        return Ok(result); 
    }
    
   
    // GET /api/attendance/consultation/{consultationId}
    // — Пријавени студенти во термин за консултации
    // Враќа:
    //
    // Id
    //     UserId
    // FirstName
    //     LastName
    // Status - како текстуална вредност
    //     Comment?
    [HttpGet("consultation/{id}")]
    public async Task<IActionResult> GetByConsultation([FromRoute] Guid id)
    {
        var result = await _attendanceMapper.GetAllByConsultationIdAsync(id);
        return Ok(result);
    }
    
    
    // PATCH /api/attendance/{id}/mark-as-absent
    // Означува отсуство на студент во термин за консултации
    // При успешно извршување враќа статус 200
    [HttpPatch("{id}/mark-as-absent")]
    public async Task<IActionResult> MarkAsAbsent([FromRoute] Guid id)
    {
        var result = await _attendanceMapper.MarkAsAbsentAsync(id);
        return Ok(result);
    }

    [HttpPost("{id}/cancelation-reason")]
    public async Task<IActionResult> UploadReasonByIdInFileSystemAsync(
        [FromRoute] Guid id,
        [FromForm] IFormFile file
    )
    {
        var result = await _attendanceMapper.UploadReasonByIdInFileSystemAsync(id, file);
        return Ok(result);
    }
    
}