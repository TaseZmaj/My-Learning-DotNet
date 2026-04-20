using EventsManagement.Domain.Entities;
using EventsManagement.Web.Mapper;
using EventsManagement.Web.Request;
using EventsManagement.Web.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace EventsManagement.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservationsController : ControllerBase
{
    private readonly ReservationMapper _reservationMapper;

    public ReservationsController(ReservationMapper reservationMapper)
    {
        _reservationMapper = reservationMapper;
    }

    [HttpGet]
    public async Task<List<ReservationResponse>> GetAllAsync()
    {
        return await _reservationMapper.GetAllAsync();
    }

    [HttpPost]
    public async Task<IActionResult> InsertAsync([FromBody] ReservationRequest request)
    {
        var result = await _reservationMapper.InsertAsync(request);
        return Ok(result);
    }

    [HttpPost("with-user")]
    public async Task<IActionResult> InsertWithUserAsync([FromBody] ReservationWithUserRequest request)
    {
        var result = await _reservationMapper.InsertAsync(request);
        return Ok(result);
    }

    [HttpPatch("confirm/{reservationId}")]
    [Authorize]
    public async Task<IActionResult> ConfirmAsync([FromRoute] Guid reservationId)
    {
        var result = await _reservationMapper.ConfirmAsync(reservationId);
        return Ok(result);
    }
}