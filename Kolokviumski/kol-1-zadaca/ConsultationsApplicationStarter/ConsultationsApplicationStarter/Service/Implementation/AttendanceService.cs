using Domain.Dto;
using Domain.Models;
using Service.Interface;

namespace Service.Implementation;

public class AttendanceService : IAttendanceService
{
    public Task<Attendance> GetByIdNotNullAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<Attendance?> GetByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<List<Attendance>> GetAllAsync(string? dateAfter)
    {
        throw new NotImplementedException();
    }

    public Task<Attendance> CreateAsync(AttendanceDto dto)
    {
        throw new NotImplementedException();
    }

    public Task<Attendance> UpdateAsync(Guid id, AttendanceDto dto)
    {
        throw new NotImplementedException();
    }

    public Task<Attendance> DeleteByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<PaginatedResult<Attendance>> GetPagedAsync(int pageNumber, int pageSize)
    {
        throw new NotImplementedException();
    }

    public Task<Attendance> UpdateReasonPathByIdAsync(Guid id, string path)
    {
        throw new NotImplementedException();
    }
}