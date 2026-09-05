using System.Text.Json.Serialization;

namespace Web.Response;

public record BasicConsultationResponse(
    Guid Id,
    Guid RoomId,
    DateTime Start,
    DateTime End
    );