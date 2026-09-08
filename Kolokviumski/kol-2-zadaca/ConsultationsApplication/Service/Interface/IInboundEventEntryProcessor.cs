using Domain.Models;

namespace Service.Interface;

public interface IInboundEventEntryProcessor
{
    public Task ProcessPendingEventsAsync();
    public Task<Attendance> ProcessEventEntry(InboundEventEntry entry);
}