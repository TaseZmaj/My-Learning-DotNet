using System.Runtime.InteropServices.JavaScript;
using Service.Interface;
using Web.Extensions;
using Web.Request;
using Web.Response;

namespace Web.Mapper;

public class ConsultationMapper
{
    private readonly IConsultationService _consultationService;

    public ConsultationMapper(IConsultationService consultationService)
    {
        _consultationService = consultationService;
    }

    public async Task<List<ConsultationResponse>> GetAllAsync(string? roomName, DateOnly? date)
    {
        var result = await _consultationService.GetAllAsync(roomName, date);
        return result.Select(x => x.ToResponse()).ToList();
    }

    public async Task<ConsultationBasicResponse> InsertAsync(ConsultationRequest request)
    {
        var result = await _consultationService.CreateAsync(
            startTime: request.StartTime,
            endTime: request.EndTime,
            roomId: request.RoomId);
        
        return result.ToBasicResponse();
    }

    public async Task<ConsultationBasicResponse> UpdateAsync(Guid id, ConsultationRequest request)
    {
        var result = await _consultationService.UpdateAsync(
            id: id,
            startTime: request.StartTime,
            endTime: request.EndTime,
            roomId: request.RoomId);
        
        return result.ToBasicResponse();
    }

    public async Task<ConsultationBasicResponse> DeleteAsync(Guid id)
    {
        var result = await _consultationService.DeleteByIdAsync(id);
        return result.ToBasicResponse();
    }

    public async Task<ConsultationResponse> GetByIdAsync(Guid id)
    {
        var result = await _consultationService.GetByIdAsync(id);
        return result.ToResponse();
    }

    public async Task<PaginatedResponse<ConsultationResponse>> GetAllPaginatedAsync(PaginatedRequest request)
    {
        var result = await _consultationService.GetPagedAsync(request.PageNumber, request.PageSize);
        return result.ToPaginatedResponse(x => x.ToResponse());
    }
}