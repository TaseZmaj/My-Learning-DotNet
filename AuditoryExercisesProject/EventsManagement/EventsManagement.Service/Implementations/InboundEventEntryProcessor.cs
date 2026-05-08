using System.Text.Json;
using EventsManagement.Domain.Dto;
using EventsManagement.Domain.Entities;
using EventsManagement.Domain.Enums;
using EventsManagement.Repository.Interfaces;
using Microsoft.Extensions.Logging;
using Service.Interfaces;

namespace Service.Implementations;

public class InboundEventEntryProcessor : IInboundEventEntryProcessor
{
    private readonly IEventService _eventService;
    private readonly IVenueService _venueService;
    private readonly IRepository<EventSectionPricing> _eventSectionPricingRepository;
    private readonly IRepository<InboundEventEntry> _inboundEventEntryRepository;
    private readonly ILogger<InboundEventEntryProcessor> _logger;

    public InboundEventEntryProcessor(
        IEventService eventService, 
        IVenueService venueService, 
        IRepository<EventSectionPricing> eventSectionPricingRepository, 
        IRepository<InboundEventEntry> inboundEventEntryRepository, 
        ILogger<InboundEventEntryProcessor> logger)
    {
        _eventService = eventService;
        _venueService = venueService;
        _eventSectionPricingRepository = eventSectionPricingRepository;
        _inboundEventEntryRepository = inboundEventEntryRepository;
        _logger = logger;
    }

    public async Task ProcessPendingEventsAsync()
    {
        // Fetch top 10 pending events ordered by receipt time
        var pending = await _inboundEventEntryRepository.GetAllAsync(
            selector: x => x,
            predicate: e => e.Status == InboundEventStatus.Pending,
            orderBy: q => q.OrderBy(e => e.ReceivedAt),
            take: 10
            );

        foreach (var entry in pending)
        {
            try
            {
                // Mark as processing to prevent other workers from picking it up
                entry.Status = InboundEventStatus.Processing;
                await _inboundEventEntryRepository.UpdateAsync(entry);

                await ProcessSingleEventAsync(entry);

                entry.Status = InboundEventStatus.Completed;
                entry.ProcessedAt = DateTime.UtcNow;

                _logger.LogInformation("Processed {Id} for Event {EventId}", 
                    entry.Id, entry.CreatedEventId);
            }
            catch (Exception ex)
            {
                entry.Status = InboundEventStatus.Failed;
                entry.ErrorMessage = ex.Message;
                entry.ProcessedAt = DateTime.UtcNow;

                _logger.LogError(ex, "Failed to process {Id}", entry.Id);
            }

            // Save the final status (Completed or Failed)
            await _inboundEventEntryRepository.UpdateAsync(entry);
        }
    }

    private async Task ProcessSingleEventAsync(InboundEventEntry entry)
    {
        var request = JsonSerializer.Deserialize<InboundEventRequest>(entry.RawPayload);

        if (request == null) throw new InvalidOperationException("Failed to deserialize");

        // 1. Match venue by name
        var venue = await _venueService.GetByNameAndCityAsync(request.VenueName, request.VenueCity);

        if (venue == null) throw new InvalidOperationException($"Venue not found: {request.VenueName}");
        
        
        // 2. Check duplicates (same title, venue, and start date)
        var existing = await _eventService.GetByTitleVenueIdAndStartDate(request.Title, venue.Id, request.StartDate);

        if (existing != null) throw new InvalidOperationException("Duplicate event");

        // 3. Create the Event
        var createdEvent = await _eventService.InsertAsync(new EventDto()
        {
            Title = request.Title,
            Description = request.Description,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            VenueId = venue.Id,
            Status = EventStatus.Draft
        });

        // 4. Set up section pricing
        foreach (var p in request.Pricing)
        {
            var section = venue.Sections
                .FirstOrDefault(s => s.Name.Equals(p.SectionName, StringComparison.OrdinalIgnoreCase));

            if (section == null) continue;

            await _eventSectionPricingRepository.InsertAsync(new EventSectionPricing()
            {
                Id = Guid.NewGuid(),
                EventId = createdEvent.Id,
                SectionId = section.Id,
                Price = p.Price,
                Currency = p.Currency
            });
        }

        // Na inboundEntry entity-to mu go dodavash Id-to na novokreiranoto Event entity-to 
        entry.CreatedEventId = createdEvent.Id;
    }
}