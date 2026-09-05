using Domain.Dto;
using Domain.Enums;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Repository;
using Service.Interface;
using TestExamIS.Tests.Utils;

namespace TestExamIS.Tests.ServicesITests;

[Collection("Test Suite")]
public class AttendanceServiceIntegrationTests : LoggedTestBase, IAsyncLifetime
{
    private readonly IAttendanceService _service;
    private readonly IConsultationService _consultationService;
    private readonly ApplicationDbContext _context;
    private readonly IServiceProvider _serviceProvider;

    public AttendanceServiceIntegrationTests(GlobalTestFixture fixture) : base(fixture)
    {
        var serviceCollection = new ServiceCollection();
        TestDatabaseHelper.SetupDI(serviceCollection);

        _serviceProvider = serviceCollection.BuildServiceProvider();
        _context = _serviceProvider.GetRequiredService<ApplicationDbContext>();
        _service = _serviceProvider.GetRequiredService<IAttendanceService>();
        _consultationService = _serviceProvider.GetRequiredService<IConsultationService>();
        ArchitectureChecker.CheckServiceForOnionArchitectureCompliance("AttendanceService");
    }

    public async Task InitializeAsync()
    {
        await TestDatabaseHelper.ResetDatabaseAsync(_serviceProvider);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [LoggedFact(Category = "AttendanceService", Points = 5)]
    public async Task Service_Should_ReturnAllAttendances()
    {
        await RunTestAsync(async () =>
        {
            var result = await _service.GetAllAsync(null);
            Assert.NotNull(result);
            Assert.True(result.Count > 0, "Should return at least one attendance.");
        });
    }

    [LoggedFact(Category = "AttendanceService", Points = 5)]
    public async Task Service_Should_GetAttendanceById()
    {
        await RunTestAsync(async () =>
        {
            var existing = await _context.Attendances.FirstAsync();
            var result = await _service.GetByIdAsync(existing.Id);
            Assert.NotNull(result);
            Assert.Equal(existing.Id, result!.Id);
        });
    }

    [LoggedFact(Category = "AttendanceService", Points = 5)]
    public async Task Service_Should_ReturnNullForNonExistingId()
    {
        await RunTestAsync(async () =>
        {
            var result = await _service.GetByIdAsync(Guid.NewGuid());
            Assert.Null(result);
        });
    }

    [LoggedFact(Category = "AttendanceService", Points = 5)]
    public async Task Service_Should_CreateAttendance()
    {
        await RunTestAsync(async () =>
        {
            var consultation = await _context.Consultations.FirstAsync();
            var user = await _context.Users.OfType<ConsultationsApplicationUser>().FirstAsync();
            var room = await _context.Rooms.FirstAsync();

            var dto = new AttendanceDto
            {
                Comment = "Test attendance",
                UserId = user.Id,
                RoomId = room.Id,
                ConsultationId = consultation.Id
            };

            var result = await _service.CreateAsync(dto);
            Assert.NotNull(result);
            Assert.Equal(Status.Registered, result.Status);
            Assert.Equal(dto.Comment, result.Comment);
        });
    }

    [LoggedFact(Category = "AttendanceService", Points = 5)]
    public async Task Service_Should_IncrementConsultationStudentsOnCreate()
    {
        await RunTestAsync(async () =>
        {
            var room = await _context.Rooms.FirstAsync();
            var consultation = await _consultationService.CreateAsync(
                DateTime.UtcNow.AddDays(1),
                DateTime.UtcNow.AddDays(1).AddHours(1),
                room.Id);
            var user = await _context.Users.OfType<ConsultationsApplicationUser>().FirstAsync();

            var initialStudents = consultation.RegisteredStudents;

            await _service.CreateAsync(new AttendanceDto
            {
                Comment = "test",
                UserId = user.Id,
                RoomId = room.Id,
                ConsultationId = consultation.Id
            });

            var updated = await _consultationService.GetByIdNotNullAsync(consultation.Id);
            Assert.Equal(initialStudents + 1, updated.RegisteredStudents);
        });
    }

    [LoggedFact(Category = "AttendanceService", Points = 5)]
    public async Task Service_Should_UpdateAttendance()
    {
        await RunTestAsync(async () =>
        {
            var existing = await _context.Attendances.FirstAsync();

            var dto = new AttendanceDto
            {
                Comment = "Updated comment",
                UserId = existing.UserId,
                RoomId = existing.RoomId,
                ConsultationId = existing.ConsultationId
            };

            var result = await _service.UpdateAsync(existing.Id, dto);
            Assert.Equal("Updated comment", result.Comment);
        });
    }

    [LoggedFact(Category = "AttendanceService", Points = 5)]
    public async Task Service_Should_DeleteAttendance()
    {
        await RunTestAsync(async () =>
        {
            var attendanceToDelete = await _context.Attendances.FirstAsync(x =>
                x.Consultation.RegisteredStudents == 0 && DateOnly.FromDateTime(x.Consultation.StartTime) >
                DateOnly.FromDateTime(DateTime.Now.AddHours(1)));


            var initialCount = await _context.Set<Attendance>().CountAsync();
            var deleted = await _service.DeleteByIdAsync(attendanceToDelete.Id);
            Assert.Equal(attendanceToDelete.Id, deleted.Id);

            var finalCount = await _context.Set<Attendance>().CountAsync();
            Assert.Equal(initialCount - 1, finalCount);
        });
    }

    [LoggedFact(Category = "AttendanceService", Points = 5)]
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

    [LoggedFact(Category = "AttendanceService", Points = 5)]
    public async Task Service_Should_UpdateReasonPath()
    {
        await RunTestAsync(async () =>
        {
            var existing = await _context.Attendances.FirstAsync();
            var path = "/uploads/cancellations/test.pdf";

            var result = await _service.UpdateReasonPathByIdAsync(existing.Id, path);
            Assert.Equal(path, result.CancellationReasonDocumentPath);
        });
    }
}