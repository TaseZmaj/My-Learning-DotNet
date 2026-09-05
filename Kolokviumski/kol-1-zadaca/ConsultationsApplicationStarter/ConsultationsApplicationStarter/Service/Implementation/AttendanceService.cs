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
    private readonly IRepository<Consultation> _consultationRepository;
    private readonly IConsultationService _consultationService;
    
    public AttendanceService(
        IRepository<Attendance> attendanceRepository,
        IConsultationService consultationService,
        IRepository<Consultation> consultationRepository
        )
    {
        _attendanceRepository = attendanceRepository;
        _consultationService = consultationService;
        _consultationRepository = consultationRepository;
    }
    
    public async Task<Attendance> GetByIdNotNullAsync(Guid id)
    {
        var result = await _attendanceRepository.GetAsync(
            selector: x => x,
            predicate: x => x.Id == id,
            include: x => x.Include(a => a.User));

        if (result == null)
        {
            throw new InvalidOperationException($"Consultation with id {id}");
        }
        
        return result;
    }

    public async Task<Attendance?> GetByIdAsync(Guid id)
    {
        return await _attendanceRepository.GetAsync(
            selector: x => x,
            predicate: x => x.Id == id
        );
    }

    public async Task<List<Attendance>> GetAllAsync(string? dateAfter)
    {
        var result = new List<Attendance>();
        
        if (dateAfter != null)
        {
            result = await _attendanceRepository.GetAllAsync(
                selector: x => x,
                predicate: x => x.Consultation.StartTime <= DateTime.Parse(dateAfter));
        }
        else
        {
            result = await _attendanceRepository.GetAllAsync(x => x);
        }
         
        return result.ToList();
    }

    public async Task<List<Attendance>> GetAllByConsultationIdAsync(Guid id)
    {
        var result = await _attendanceRepository.GetAllAsync(
                selector: x => x,
                predicate: x => x.ConsultationId == id);
        
        return result.ToList();
    }
    
    public async Task<Attendance> CreateAsync(AttendanceDto dto)
    {
        var attendanceToAdd = new Attendance
        {
            Comment = dto.Comment,
            UserId = dto.UserId,
            RoomId = dto.RoomId,
            ConsultationId = dto.ConsultationId,
            Status = Status.Registered
        };
        
        var result = await _attendanceRepository.InsertAsync(attendanceToAdd);
        
        await _consultationService.IncrementRegisteredStudents(dto.ConsultationId);
        
        return await GetByIdNotNullAsync(result.Id);
    }

    public async Task<Attendance> UpdateAsync(Guid id, AttendanceDto dto)
    {
        var attendanceToUpdate = await GetByIdNotNullAsync(id);
        
        attendanceToUpdate.Comment = dto.Comment;
        attendanceToUpdate.RoomId = dto.RoomId;
        attendanceToUpdate.ConsultationId = dto.ConsultationId;
        attendanceToUpdate.UserId = dto.UserId;
        
        return await _attendanceRepository.UpdateAsync(attendanceToUpdate);
    }

    public async Task<Attendance> DeleteByIdAsync(Guid id)
    {
        var attendanceToDelete = await GetByIdNotNullAsync(id);
        var consultation = await _consultationService.GetByIdNotNullAsync(attendanceToDelete.ConsultationId);
        
        if (consultation.StartTime <= DateTime.Now.AddHours(1))
        {
            throw new InvalidOperationException($"Attendance with id {id} is in 1 hour or less, can't delete.");
        }

        await _consultationService.DecrementRegisteredStudents(attendanceToDelete.ConsultationId);
        
        await _attendanceRepository.DeleteAsync(attendanceToDelete);

        return attendanceToDelete;
    }

    public async Task<PaginatedResult<Attendance>> GetPagedAsync(int pageNumber, int pageSize)
    {
        return await _attendanceRepository.GetAllPagedAsync(
            selector: x => x,
            pageNumber: pageNumber,
            pageSize: pageSize,
            orderBy: x => x.OrderBy(e => e.Id),
            asNoTracking: true);
    }

    public async Task<Attendance> UpdateReasonPathByIdAsync(Guid id, string path)
    {
        var attendanceToUpdate = await GetByIdNotNullAsync(id);
        
        attendanceToUpdate.CancellationReasonDocumentPath = path;
        return await _attendanceRepository.UpdateAsync(attendanceToUpdate);
    }

    public async Task<Attendance> MarkAsAbsent(Guid id)
    {
        var attendanceToUpdate = await GetByIdNotNullAsync(id);
        
        attendanceToUpdate.Status = Status.Absent;
        
        return await _attendanceRepository.UpdateAsync(attendanceToUpdate);
    }
}