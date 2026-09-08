using Domain.Dto;
using Domain.Enums;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Repository.Interface;
using Service.Interface;

namespace Service.Implementation;

public class AttendanceService : IAttendanceService
{
    
    private readonly IRepository<Attendance> _attendanceRepository;
    private readonly IConsultationService _consultationService;

    public AttendanceService(IRepository<Attendance> attendanceRepository, IConsultationService consultationService)
    {
        _attendanceRepository = attendanceRepository;
        _consultationService = consultationService;
    }

    public async Task<Attendance> GetByIdNotNullAsync(Guid id)
    {
        var result = await GetByIdAsync(id);

        if (result == null)
        {
            throw new InvalidOperationException($"Attendance with ID: {id} not found");
        }
        
        return result;
    }

    public async Task<Attendance?> GetByIdAsync(Guid id)
    {
        return await _attendanceRepository.GetAsync(
            selector: x => x,
            predicate: x => x.Id == id);
    }

    public async Task<List<Attendance>> GetAllAsync(string? dateAfter)
    {
        return await _attendanceRepository.GetAllAsync(
            selector: x => x);
    }

    public async Task<Attendance> CreateAsync(AttendanceDto dto)
    {
        var attendance = new Attendance
        {
            Comment = dto.Comment,
            ConsultationId = dto.ConsultationId,
            RoomId = dto.RoomId,
            Status = Status.Registered,
            UserId = dto.UserId
        };
        
        var result = await _attendanceRepository.InsertAsync(attendance);
        await _consultationService.IncrementNumberOfStudentsAsync(dto.ConsultationId);
        
        var test = _consultationService.GetByIdNotNullAsync(result.ConsultationId);
        
        return await GetByIdNotNullAsync(result.Id);
    }

    public async Task<Attendance> UpdateAsync(Guid id, AttendanceDto dto)
    {
        var attendance = await GetByIdNotNullAsync(id);
        
        attendance.Comment = dto.Comment;
        attendance.ConsultationId = dto.ConsultationId;
        attendance.RoomId = dto.RoomId;
        attendance.UserId = dto.UserId;
        
        return await _attendanceRepository.UpdateAsync(attendance);
    }

    public async Task<Attendance> DeleteByIdAsync(Guid id)
    {
        var attendance = await GetByIdNotNullAsync(id);
        
        var consultation = await _consultationService.GetByIdNotNullAsync(attendance.ConsultationId);

        if (consultation.RegisteredStudents > 0)
        {
            throw new InvalidOperationException($"Attendances already registered for this consultation");
        }

        if (consultation.StartTime <= DateTime.Now.AddHours(1))
        {
            throw new InvalidOperationException($"This consultation cannot be deleted because it starts in less than 1 hour.");
        }
        
        await _consultationService.DecrementNumberOfStudentsAsync(consultation.Id);
        
        await _attendanceRepository.DeleteAsync(attendance);
        
        return attendance;
    }

    public async Task<PaginatedResult<Attendance>> GetPagedAsync(int pageNumber, int pageSize)
    {
        return await _attendanceRepository.GetAllPagedAsync(
            selector: x => x,
            pageNumber: pageNumber,
            pageSize: pageSize);
    }

    public async Task<Attendance> UpdateReasonPathByIdAsync(Guid id, string path)
    {
        var attendance = await GetByIdNotNullAsync(id);
        attendance.CancellationReasonDocumentPath = path;
        return await _attendanceRepository.UpdateAsync(attendance);
    }

    public async Task<List<Attendance>> GetAllByConsultationIdAsync(Guid consultationId)
    {
        return await _attendanceRepository.GetAllAsync(
            selector: x => x,
            predicate: x => x.ConsultationId == consultationId,
            include: x => x.Include(a => a.User));
        
    }

    public async Task MarkAsAbsentByIdAsync(Guid id)
    {
        var result = await GetByIdNotNullAsync(id);
        result.Status = Status.Absent;
        await _attendanceRepository.UpdateAsync(result);
    }
}