using Service.Interface;
using Web.Request;
using Web.Response;
using Web.Extensions;

namespace Web.Mapper;

public class ConsultationMapper
{
    private readonly IConsultationService _consultationService;

    public ConsultationMapper(
        IConsultationService consultationService
        )
    {
        _consultationService = consultationService;
    }
    
    public async Task<List<ConsultationResponse>> GetAllAsync(string? roomName, DateOnly? date)
    {
        var result = await _consultationService.GetAllAsync(roomName, date);
        return result.ToResponse();
    }
    
    public async Task<BasicConsultationResponse> InsertAsync(ConsultationRequest request)
    {
        
        var result = await _consultationService.CreateAsync(
            request.StartTime,
            request.EndTime,
            request.RoomId);
        
        return result.ToBasicResponse();
    }

    public async Task<BasicConsultationResponse> UpdateAsync(Guid id, ConsultationRequest request)
    {
        var result = await _consultationService.UpdateAsync(
            id, 
            request.StartTime, 
            request.EndTime, 
            request.RoomId);
        
        return result.ToBasicResponse();
    }
    
    public async Task<BasicConsultationResponse> DeleteAsync(Guid id)
    {
        var result = await _consultationService.DeleteByIdAsync(id);
        
        return result.ToBasicResponse();
    }
    
    public async Task<PaginatedResponse<ConsultationResponse>> PaginatedGetAllAsync(PaginatedRequest request)
    {
        var result = await _consultationService.GetPagedAsync(request.PageNumber, request.PageSize);
        return result.ToPaginatedResponse(e => e.ToResponse());
    }
}