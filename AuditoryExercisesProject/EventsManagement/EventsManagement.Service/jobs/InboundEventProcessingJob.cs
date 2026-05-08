using Microsoft.Extensions.Logging;
using Quartz;
using Service.Interfaces;

namespace Service.jobs;

[DisallowConcurrentExecution]
public class InboundEventProcessingJob : IJob
{
    private readonly IInboundEventEntryProcessor _inboundEventEntryProcessor;
    private readonly ILogger<InboundEventProcessingJob> _logger;

    public InboundEventProcessingJob(
        IInboundEventEntryProcessor inboundEventEntryProcessor, 
        ILogger<InboundEventProcessingJob> logger)
    {
        _inboundEventEntryProcessor = inboundEventEntryProcessor;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            _logger.LogInformation("Starting Inbound Event Processing job...");
            await _inboundEventEntryProcessor.ProcessPendingEventsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Inbound event processing failed");
            throw new JobExecutionException(ex,
                refireImmediately:false);
        }
    }
}