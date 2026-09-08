using Domain.Dto;
using Domain.Models;
using Microsoft.EntityFrameworkCore.Update;
using Service.Interface;
using Web.Extensions;
using Web.Request;
using Web.Response;

namespace Web.Mapper;

public class AttendanceMapper
{
    private readonly IAttendanceService _attendanceService;
    private readonly IFileUploadService _fileUploadService;

    public AttendanceMapper(IAttendanceService attendanceService, IFileUploadService fileUploadService)
    {
        _attendanceService = attendanceService;
        _fileUploadService = fileUploadService;
    }

    public async Task<AttendanceResponse> RegisterAsync(AttendanceRequest request)
    {
        var result = await _attendanceService.CreateAsync(new AttendanceDto()
        {
            Comment = request.Comment,
            ConsultationId = request.ConsultationId,
            UserId = request.UserId,
            RoomId = request.RoomId,
        });

        return result.ToResponse();
    }

    public async Task DeleteAsync(Guid id)
    {
        await _attendanceService.DeleteByIdAsync(id);
    }

    public async Task<List<AttendanceResponse>> GetAllByConsultationIdAsync(Guid consultationId)
    {
        var result = await _attendanceService.GetAllByConsultationIdAsync(consultationId);
        return result.Select(x => x.ToResponse()).ToList();
    }

    public async Task MarkAsAbsentAsync(Guid id)
    {
        await _attendanceService.MarkAsAbsentByIdAsync(id);
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
