using Service.Implementation;
using Service.Interface;
using Web.Extensions;
using Web.Request;
using Web.Response;

namespace Web.Mapper;

public class AttendanceMapper
{
    private readonly IAttendanceService _attendanceService;
    private readonly IFileUploadService _fileUploadService;

    public AttendanceMapper(
        IAttendanceService attendanceService,
        IFileUploadService fileUploadService)
    {
        _attendanceService = attendanceService;
        _fileUploadService = fileUploadService;
    }
    
    public async Task<AttendanceResponse?> GetById(Guid id)
    {
        var result = await _attendanceService.GetByIdAsync(id);
        return result?.ToResponse();
    }

    public async Task<List<AttendanceResponse>> GetAll(string id)
    {
        var result = await _attendanceService.GetAllAsync(id);
        return result.ToResponse();
    }
    
    public async Task<List<AttendanceResponse>> GetAllByConsultationIdAsync(Guid id)
    {
        var result = await _attendanceService.GetAllByConsultationIdAsync(id);
        return result.ToResponse();
    }

    public async Task<AttendanceResponse> RegisterAsync(AttendanceRequest request)
    {
        var result = await _attendanceService.CreateAsync(request.ToDto());
        return result.ToResponse();
    }

    public async Task<AttendanceResponse> UpdateAsync(Guid id, AttendanceRequest request)
    {
        var dto = request.ToDto();
        
        var result = await _attendanceService.UpdateAsync(id, dto);
        
        return result.ToResponse();
    }

    public async Task<AttendanceResponse> MarkAsAbsentAsync(Guid id)
    {
        var result = await _attendanceService.MarkAsAbsent(id);
        return result.ToResponse();
    }

    public async Task<AttendanceResponse> DeleteAsync(Guid id)
    {
        var result = await _attendanceService.DeleteByIdAsync(id);
        return result.ToResponse();
    }
    
    public async Task<PaginatedResponse<AttendanceResponse>> PaginatedGetAllAsync(PaginatedRequest request)
    {
        var result = await _attendanceService.GetPagedAsync(request.PageNumber, request.PageSize);
        return result.ToPaginatedResponse(a => a.ToResponse());
    }

    public async Task<AttendanceResponse> UploadReasonByIdInFileSystemAsync(Guid id, IFormFile file)
    {
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);

        var path = await _fileUploadService.UploadFileAsync(
            ms.ToArray(),
            file.FileName
        );

        var result = await _attendanceService.UpdateReasonPathByIdAsync(id, path);

        return result.ToResponse();
    }
}