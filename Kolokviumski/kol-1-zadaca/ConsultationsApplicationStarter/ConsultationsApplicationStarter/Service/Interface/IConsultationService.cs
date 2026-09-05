using Domain.Dto;
using Domain.Models;

namespace Service.Interface;
public interface IConsultationService
{
    Task<Consultation> GetByIdNotNullAsync(Guid id);
    Task<Consultation?> GetByIdAsync(Guid id);
    Task<List<Consultation>> GetAllAsync(string? roomName, DateOnly? date);
    Task<Consultation> CreateAsync(DateTime startTime, DateTime endTime, Guid roomId);
    Task<Consultation> UpdateAsync(Guid id, DateTime startTime, DateTime endTime, Guid roomId);
    Task<Consultation> DeleteByIdAsync(Guid id);
    Task<PaginatedResult<Consultation>> GetPagedAsync(int pageNumber, int pageSize);

}