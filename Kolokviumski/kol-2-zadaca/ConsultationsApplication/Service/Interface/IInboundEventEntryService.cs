namespace Service.Interface;

public interface IInboundEventEntryService
{
    Task<InboundEventEntry> CreateAsync(string rawPayload);
    Task<InboundEventEntry> GetVyIdNotNullAsync(Guid id);
}