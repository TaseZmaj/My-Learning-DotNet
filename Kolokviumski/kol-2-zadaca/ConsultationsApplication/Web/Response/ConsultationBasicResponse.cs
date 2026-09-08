namespace Web.Response;

public record ConsultationBasicResponse(
    Guid Id,
    Guid RoomId,
    DateTime Start,
    DateTime End
);