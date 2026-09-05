using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Repository;
using Service.Interface;
using TestExamIS.Tests.Utils;

namespace TestExamIS.Tests.ServicesITests;

[Collection("Test Suite")]
public class ConsultationServiceIntegrationTests : LoggedTestBase, IAsyncLifetime
{
    private readonly IConsultationService _service;
    private readonly ApplicationDbContext _context;
    private readonly IServiceProvider _serviceProvider;

    public ConsultationServiceIntegrationTests(GlobalTestFixture fixture) : base(fixture)
    {
        var serviceCollection = new ServiceCollection();
        TestDatabaseHelper.SetupDI(serviceCollection);

        _serviceProvider = serviceCollection.BuildServiceProvider();
        _context = _serviceProvider.GetRequiredService<ApplicationDbContext>();
        _service = _serviceProvider.GetRequiredService<IConsultationService>();
        ArchitectureChecker.CheckServiceForOnionArchitectureCompliance("ConsultationService");
    }

    public async Task InitializeAsync()
    {
        await TestDatabaseHelper.ResetDatabaseAsync(_serviceProvider);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [LoggedFact(Category = "ConsultationService", Points = 5)]
    public async Task Service_Should_ReturnAllConsultations()
    {
        await RunTestAsync(async () =>
        {
            var result = await _service.GetAllAsync(null, null);
            Assert.NotNull(result);
            Assert.True(result.Count > 0, "Should return at least one consultation.");
        });
    }

    [LoggedFact(Category = "ConsultationService", Points = 5)]
    public async Task Service_Should_FilterByRoomName()
    {
        await RunTestAsync(async () =>
        {
            var result = await _service.GetAllAsync("A101", null);
            Assert.NotNull(result);
            Assert.All(result, c => Assert.Equal("A101", c.Room.Name));
        });
    }    
    
    [LoggedFact(Category = "ConsultationService", Points = 5)]
    public async Task Service_Should_FilterByRoomNameAndDate()
    {
        await RunTestAsync(async () =>
        {
            var result = await _service.GetAllAsync("A101", new DateOnly(2026, 4, 13));
            Assert.NotNull(result);
            Assert.All(result, c => Assert.Equal("A101", c.Room.Name));
            Assert.All(result, c => Assert.Equal(new DateOnly(2026, 4, 13), DateOnly.FromDateTime(c.StartTime)));
        });
    }    
    
    [LoggedFact(Category = "ConsultationService", Points = 5)]
    public async Task Service_Should_FilterByDate()
    {
        await RunTestAsync(async () =>
        {
            var result = await _service.GetAllAsync(null, new DateOnly(2026, 4, 13));
            Assert.NotNull(result);
            Assert.All(result, c => Assert.Equal(new DateOnly(2026, 4, 13), DateOnly.FromDateTime(c.StartTime)));
        });
    }

    [LoggedFact(Category = "ConsultationService", Points = 5)]
    public async Task Service_Should_GetConsultationById()
    {
        await RunTestAsync(async () =>
        {
            var room = await _context.Rooms.FirstAsync();
            var created = await _service.CreateAsync(
                DateTime.UtcNow.AddDays(1),
                DateTime.UtcNow.AddDays(1).AddHours(2),
                room.Id);

            var result = await _service.GetByIdAsync(created.Id);
            Assert.NotNull(result);
            Assert.Equal(created.Id, result!.Id);
        });
    }

    [LoggedFact(Category = "ConsultationService", Points = 5)]
    public async Task Service_Should_ReturnNullForNonExistingId()
    {
        await RunTestAsync(async () =>
        {
            var result = await _service.GetByIdAsync(Guid.NewGuid());
            Assert.Null(result);
        });
    }

    [LoggedFact(Category = "ConsultationService", Points = 5)]
    public async Task Service_Should_ThrowForNonExistingIdNotNull()
    {
        await RunTestAsync(async () =>
        {
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.GetByIdNotNullAsync(Guid.NewGuid()));
        });
    }

    [LoggedFact(Category = "ConsultationService", Points = 5)]
    public async Task Service_Should_CreateConsultation()
    {
        await RunTestAsync(async () =>
        {
            var initialCount = await _context.Set<Consultation>().CountAsync();
            var room = await _context.Rooms.FirstAsync();

            var result = await _service.CreateAsync(
                DateTime.UtcNow.AddDays(5),
                DateTime.UtcNow.AddDays(5).AddHours(2),
                room.Id);

            Assert.NotNull(result);
            Assert.Equal(room.Id, result.RoomId);

            var finalCount = await _context.Set<Consultation>().CountAsync();
            Assert.Equal(initialCount + 1, finalCount);
        });
    }

    [LoggedFact(Category = "ConsultationService", Points = 5)]
    public async Task Service_Should_UpdateConsultation()
    {
        await RunTestAsync(async () =>
        {
            var room = await _context.Rooms.FirstAsync();
            var created = await _service.CreateAsync(
                DateTime.UtcNow.AddDays(1),
                DateTime.UtcNow.AddDays(1).AddHours(1),
                room.Id);

            var newRoom = await _context.Rooms.Skip(1).FirstAsync();
            var newStart = DateTime.UtcNow.AddDays(10);
            var newEnd = newStart.AddHours(3);

            var updated = await _service.UpdateAsync(created.Id, newStart, newEnd, newRoom.Id);

            Assert.Equal(newRoom.Id, updated.RoomId);
            Assert.Equal(newStart, updated.StartTime);
            Assert.Equal(newEnd, updated.EndTime);
        });
    }

    [LoggedFact(Category = "ConsultationService", Points = 5)]
    public async Task Service_Should_DeleteConsultation()
    {
        await RunTestAsync(async () =>
        {
            var room = await _context.Rooms.FirstAsync();
            var created = await _service.CreateAsync(
                DateTime.UtcNow.AddDays(1),
                DateTime.UtcNow.AddDays(1).AddHours(1),
                room.Id);

            var initialCount = await _context.Set<Consultation>().CountAsync();

            var deleted = await _service.DeleteByIdAsync(created.Id);
            Assert.Equal(created.Id, deleted.Id);

            var finalCount = await _context.Set<Consultation>().CountAsync();
            Assert.Equal(initialCount - 1, finalCount);
        });
    }

    [LoggedFact(Category = "ConsultationService", Points = 5)]
    public async Task Service_Should_GetPagedResults()
    {
        await RunTestAsync(async () =>
        {
            var result = await _service.GetPagedAsync(0, 5);

            Assert.NotNull(result);
            Assert.True(result.Items.Count <= 5);
            Assert.True(result.TotalCount > 0);
        });
    }
}
