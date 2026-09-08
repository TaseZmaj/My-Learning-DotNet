using System.Text.Json;
using Domain.Enums;
using Domain.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Service.Interface;
using TestExamIS.Tests.Utils;

namespace TestExamIS.Tests.ExternalApiTests;

[Collection("Test Suite")]
public class InboundEventProcessorTests : LoggedTestBase, IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private const string DevApiKey = "HyeORCiubWiUO4E1m1h3dGPjPKWhND1f";

    public InboundEventProcessorTests(WebApplicationFactory<Program> factory, GlobalTestFixture fixture) : base(fixture)
    {
        _factory = factory
            .WithTestDatabase()
            .WithWebHostBuilder(b => b.ConfigureAppConfiguration((_, cfg) =>
                cfg.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ApiKeySettings:ApiKey"] = DevApiKey
                })));
    }

    public async Task InitializeAsync() => await TestDatabaseHelper.ResetDatabaseAsync(_factory.Services);
    public Task DisposeAsync() => Task.CompletedTask;

    private static string MakePayload(Guid consultationId, string userId, Guid roomId, string? comment = null) =>
        JsonSerializer.Serialize(new { consultationId, userId, roomId, comment });

    private async Task<InboundEventEntry> CreatePendingEntry(Guid consultationId, string userId, Guid roomId)
    {
        var entry = new InboundEventEntry
        {
            Id = Guid.NewGuid(),
            RawPayload = MakePayload(consultationId, userId, roomId),
            Status = InboundEventStatus.Pending,
            ReceivedAt = DateTime.UtcNow
        };
        return await TestDatabaseHelper.CreateEntity(_factory.Services, entry);
    }

    [LoggedFact(Category = "InboundEvent", Points = 8)]
    public async Task ProcessEventEntry_ValidPayload_ShouldCreateAttendance()
    {
        await RunTestAsync(async () =>
        {
            var consultation = await TestDatabaseHelper.GetFirst<Consultation>(_factory.Services);
            var room = await TestDatabaseHelper.GetFirst<Room>(_factory.Services);
            var entry = await CreatePendingEntry(consultation.Id, "test-user-1", room.Id);

            var countBefore = await TestDatabaseHelper.GetCount<Attendance>(_factory.Services);

            using var scope = _factory.Services.CreateScope();
            var processor = scope.ServiceProvider.GetRequiredService<IInboundEventEntryProcessor>();
            var attendance = await processor.ProcessEventEntry(entry);

            var countAfter = await TestDatabaseHelper.GetCount<Attendance>(_factory.Services);
            Assert.NotNull(attendance);
            Assert.Equal(countBefore + 1, countAfter);
        });
    }

    [LoggedFact(Category = "InboundEvent", Points = 5)]
    public async Task ProcessEventEntry_ValidPayload_ShouldSetEntryToCompleted()
    {
        await RunTestAsync(async () =>
        {
            var consultation = await TestDatabaseHelper.GetFirst<Consultation>(_factory.Services);
            var room = await TestDatabaseHelper.GetFirst<Room>(_factory.Services);
            var entry = await CreatePendingEntry(consultation.Id, "test-user-2", room.Id);

            using var scope = _factory.Services.CreateScope();
            var processor = scope.ServiceProvider.GetRequiredService<IInboundEventEntryProcessor>();
            var attendance = await processor.ProcessEventEntry(entry);

            var updatedEntry = await TestDatabaseHelper.GetFirstWhere<InboundEventEntry>(
                _factory.Services, e => e.Id == entry.Id);

            Assert.NotNull(updatedEntry);
            Assert.Equal(InboundEventStatus.Completed, updatedEntry!.Status);
            Assert.NotNull(updatedEntry.ProcessedAt);
            Assert.Equal(attendance.Id, updatedEntry.AttendanceId);
        });
    }

    [LoggedFact(Category = "InboundEvent", Points = 5)]
    public async Task ProcessEventEntry_InvalidPayload_ShouldSetEntryToFailed()
    {
        await RunTestAsync(async () =>
        {
            var entry = new InboundEventEntry
            {
                Id = Guid.NewGuid(),
                RawPayload = "{ this is not valid json {{ ",
                Status = InboundEventStatus.Pending,
                ReceivedAt = DateTime.UtcNow
            };
            await TestDatabaseHelper.CreateEntity(_factory.Services, entry);

            var countBefore = await TestDatabaseHelper.GetCount<Attendance>(_factory.Services);

            using var scope = _factory.Services.CreateScope();
            var processor = scope.ServiceProvider.GetRequiredService<IInboundEventEntryProcessor>();
            await processor.ProcessEventEntry(entry);

            var countAfter = await TestDatabaseHelper.GetCount<Attendance>(_factory.Services);
            Assert.Equal(countBefore, countAfter);

            var updatedEntry = await TestDatabaseHelper.GetFirstWhere<InboundEventEntry>(
                _factory.Services, e => e.Id == entry.Id);
            Assert.NotNull(updatedEntry);
            Assert.Equal(InboundEventStatus.Failed, updatedEntry!.Status);
            Assert.NotNull(updatedEntry.ErrorMessage);
        });
    }

    [LoggedFact(Category = "InboundEvent", Points = 8)]
    public async Task ProcessPendingEvents_ShouldProcessAllPendingEntries()
    {
        await RunTestAsync(async () =>
        {
            var consultation = await TestDatabaseHelper.GetFirst<Consultation>(_factory.Services);
            var room = await TestDatabaseHelper.GetFirst<Room>(_factory.Services);

            await CreatePendingEntry(consultation.Id, "test-user-5", room.Id);
            await CreatePendingEntry(consultation.Id, "test-user-6", room.Id);
            await CreatePendingEntry(consultation.Id, "test-user-7", room.Id);

            var attendanceCountBefore = await TestDatabaseHelper.GetCount<Attendance>(_factory.Services);

            using var scope = _factory.Services.CreateScope();
            var processor = scope.ServiceProvider.GetRequiredService<IInboundEventEntryProcessor>();
            await processor.ProcessPendingEventsAsync();

            var attendanceCountAfter = await TestDatabaseHelper.GetCount<Attendance>(_factory.Services);
            Assert.Equal(attendanceCountBefore + 3, attendanceCountAfter);

            var stillPending = await TestDatabaseHelper.GetAllWhere<InboundEventEntry>(
                _factory.Services,
                e => e.Status == InboundEventStatus.Pending);
            Assert.Empty(stillPending);
        });
    }

    [LoggedFact(Category = "InboundEvent", Points = 5)]
    public async Task ProcessPendingEvents_ShouldIgnoreNonPendingEntries()
    {
        await RunTestAsync(async () =>
        {
            var consultation = await TestDatabaseHelper.GetFirst<Consultation>(_factory.Services);
            var room = await TestDatabaseHelper.GetFirst<Room>(_factory.Services);

            var alreadyCompleted = new InboundEventEntry
            {
                Id = Guid.NewGuid(),
                RawPayload = MakePayload(consultation.Id, "test-user-8", room.Id),
                Status = InboundEventStatus.Completed,
                ReceivedAt = DateTime.UtcNow.AddMinutes(-5),
                ProcessedAt = DateTime.UtcNow.AddMinutes(-4)
            };
            await TestDatabaseHelper.CreateEntity(_factory.Services, alreadyCompleted);
            await CreatePendingEntry(consultation.Id, "test-user-9", room.Id);

            var attendanceCountBefore = await TestDatabaseHelper.GetCount<Attendance>(_factory.Services);

            using var scope = _factory.Services.CreateScope();
            var processor = scope.ServiceProvider.GetRequiredService<IInboundEventEntryProcessor>();
            await processor.ProcessPendingEventsAsync();

            var attendanceCountAfter = await TestDatabaseHelper.GetCount<Attendance>(_factory.Services);
            Assert.Equal(attendanceCountBefore + 1, attendanceCountAfter);
        });
    }
}
