using Domain.Dto;
using Domain.Models;
using Repository.Interface;
using Service.Interface;
using Microsoft.EntityFrameworkCore;

namespace Service.Implementation;

public class ConsultationService : IConsultationService
{
    private readonly IRepository<Consultation> _repository;
    
    public ConsultationService(IRepository<Consultation> repository)
    {
        _repository = repository;
    }
    
    public async Task<Consultation> GetByIdNotNullAsync(Guid id)
    {
        var result = await _repository.GetAsync(
            selector: x => x,
            predicate: x => x.Id == id);

        if (result == null)
        {
            throw new InvalidOperationException($"Consultation with id {id}");
        }
        
        return result;
    }

    public async Task<Consultation?> GetByIdAsync(Guid id)
    {
        return await _repository.GetAsync(
            selector: x => x,
            predicate: x => x.Id == id
                );
    }

    public async Task<List<Consultation>> GetAllAsync(string? roomName, DateOnly? date)
    {
        var result = new List<Consultation>();

        if (roomName != null && date == null)
        {
            result = await _repository.GetAllAsync(
                selector: x => x,
                predicate: x => x.Room.Name == roomName);
        }
        else if (date != null && roomName == null)
        {
            result = await _repository.GetAllAsync(
                selector: x => x,
                predicate: x => DateOnly.FromDateTime(x.StartTime) == date);

        }
        else if (date != null && roomName != null)
        {
            result = await _repository.GetAllAsync(
                selector: x => x,
                predicate: x => x.Room.Name == roomName && DateOnly.FromDateTime(x.StartTime) == date);

        }
        else
        {
            result = await _repository.GetAllAsync(x => x);
        }
         
        return result.ToList();
    }

    public async Task<Consultation> CreateAsync(DateTime startTime, DateTime endTime, Guid roomId)
    {
        
        var consultationToAdd = new Consultation
        {
            RegisteredStudents = 0,
            StartTime = startTime,
            EndTime = endTime,
            RoomId = roomId,
        };
        
        return await _repository.InsertAsync(consultationToAdd);
    }

    public async Task<Consultation> UpdateAsync(Guid id, DateTime startTime, DateTime endTime, Guid roomId)
    {
        var consultationToUpdate = await GetByIdNotNullAsync(id);
        
        consultationToUpdate.StartTime = startTime;
        consultationToUpdate.EndTime = endTime;
        consultationToUpdate.RoomId = roomId;
        
        return await _repository.UpdateAsync(consultationToUpdate);
    }

    public async Task<Consultation> DeleteByIdAsync(Guid id)
    {
        var consultationToDelete = await GetByIdNotNullAsync(id);
        
        if (consultationToDelete.RegisteredStudents > 0)
        {
            throw new InvalidOperationException($"Cannot delete consultation id={id} -> it has registered students");
        }
        
        return await _repository.DeleteAsync(consultationToDelete);

    }

    public async Task<PaginatedResult<Consultation>> GetPagedAsync(int pageNumber, int pageSize)
    {
        if(pageNumber < 0) pageNumber = 0;
        if(pageNumber > 100) pageNumber = 100;
        if (pageSize < 0) pageSize = 10;
        
        return await _repository.GetAllPagedAsync(
            selector: x => x,
            pageNumber: pageNumber,
            pageSize: pageSize,
            include: x => x.Include(y => y.Attendances),
            orderBy: x => x.OrderBy(e => e.StartTime),
            asNoTracking: true);
    }

    public async Task<Consultation> IncrementRegisteredStudents(Guid id)
    {
        var consultationToUpdate = await GetByIdNotNullAsync(id);
        consultationToUpdate.RegisteredStudents++;
        
        return await _repository.UpdateAsync(consultationToUpdate);
    }

    public async Task<Consultation> DecrementRegisteredStudents(Guid id)
    {
        var consultationToUpdate = await GetByIdNotNullAsync(id);
        consultationToUpdate.RegisteredStudents--;
        return await _repository.UpdateAsync(consultationToUpdate);
    }
}