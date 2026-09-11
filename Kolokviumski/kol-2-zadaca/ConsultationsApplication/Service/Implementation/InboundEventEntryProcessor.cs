using System.Text.Json;
using Domain.Dto;
using Domain.Enums;
using Domain.Models;
using Repository.Interface;
using Service.Interface;

namespace Service.Implementation;

public class InboundEventEntryProcessor : IInboundEventEntryProcessor
{
    private readonly IRepository<InboundEventEntry> _repository;
    private readonly IAttendanceService _attendanceService;

    public InboundEventEntryProcessor(IRepository<InboundEventEntry> repository, IAttendanceService attendanceService)
    {
        _repository = repository;
        _attendanceService = attendanceService;
    }
    
    public async Task ProcessPendingEventsAsync()
    {
        var pending = await _repository.GetAllAsync(
            selector: x => x,
            predicate: e => e.Status == InboundEventStatus.Pending,
            orderBy: q => q.OrderBy(e => e.ReceivedAt)
            );
            // take: 10);

        foreach (var entry in pending)
        {
            await ProcessEventEntry(entry);
        }
    }
    
    public async Task<Attendance> ProcessEventEntry(InboundEventEntry entry)
    {
        try
        {
            var dto = JsonSerializer.Deserialize<InboundEventRequest>(
                entry.RawPayload,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (dto == null || string.IsNullOrEmpty(dto.UserId))
                throw new InvalidOperationException("Invalid or incomplete payload");

            var attendance = await _attendanceService.CreateAsync(new AttendanceDto
            {
                ConsultationId = dto.ConsultationId,
                RoomId = dto.RoomId,
                Comment = dto.Comment,
                UserId = dto.UserId,
            });

            entry.Status = InboundEventStatus.Completed;
            entry.ProcessedAt = DateTime.UtcNow;
            entry.AttendanceId = attendance.Id;
            await _repository.UpdateAsync(entry);

            return attendance;
        }
        catch (Exception ex)
        {
            entry.Status = InboundEventStatus.Failed;
            entry.ErrorMessage = ex.Message;
            await _repository.UpdateAsync(entry);
            return null!;
        }
        
    }
}