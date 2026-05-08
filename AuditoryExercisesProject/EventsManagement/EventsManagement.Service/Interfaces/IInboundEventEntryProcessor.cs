namespace Service.Interfaces;

public interface IInboundEventEntryProcessor
{
    public Task ProcessPendingEventsAsync();
}