using System.ComponentModel.DataAnnotations;
using EventsManagement.Domain.Enums;

namespace EventsManagement.Web.Request;

public record EventRequest(
    [Required] string Title,
    /*[Required]*/ string? Description,
    [Required] DateTime StartDate,
    [Required] DateTime EndDate,
    [Required] string Status,
    string? ImageUrl,
    [Required] Guid VenueId,
    [Required] string UserId
    );

            
