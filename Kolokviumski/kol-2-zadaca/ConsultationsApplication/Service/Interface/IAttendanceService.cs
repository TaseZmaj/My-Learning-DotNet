using Domain.Dto;
using Domain.Models;

namespace Service.Interface;

public interface IAttendanceService
{
    Task<Attendance> GetByIdNotNullAsync(Guid id);
    Task<Attendance?> GetByIdAsync(Guid id);
    Task<List<Attendance>> GetAllAsync(string? dateAfter);
    Task<Attendance> CreateAsync(AttendanceDto dto);
    Task<Attendance> UpdateAsync(Guid id, AttendanceDto dto);
    Task<Attendance> DeleteByIdAsync(Guid id);
    Task<PaginatedResult<Attendance>> GetPagedAsync(int pageNumber, int pageSize);
    Task<Attendance> UpdateReasonPathByIdAsync(Guid id, string path);
    
    Task<List<Attendance>> GetAllByConsultationIdAsync(Guid consultationId);
    Task MarkAsAbsentByIdAsync(Guid id);
}