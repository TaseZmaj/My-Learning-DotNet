using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Domain.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TestExamIS.Tests.Utils;

namespace TestExamIS.Tests.ControllersTests;

[Collection("Test Suite")]
public class AttendanceControllerTests : LoggedTestBase, IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private const string ControllerName = "AttendanceController";
    private const string BaseUrl = "/api/Attendance";

    public AttendanceControllerTests(WebApplicationFactory<Program> factory, GlobalTestFixture fixture) : base(fixture)
    {
        _factory = factory.WithTestDatabase().WithTestAuth();
        _client = _factory.CreateAuthenticatedClient();
        _client.Timeout = TimeSpan.FromSeconds(5);
        ArchitectureChecker.CheckControllerForOnionArchitectureCompliance(ControllerName);
    }

    public async Task InitializeAsync()
    {
        await TestDatabaseHelper.ResetDatabaseAsync(_factory.Services);
    }

    public async Task DisposeAsync()
    {
        await TestDatabaseHelper.ResetDatabaseAsync(_factory.Services);
    }

    [LoggedFact(Category = "AttendanceController", Points = 10)]
    public async Task Register_ValidRequest_ShouldCreateAndReturnFields()
    {
        await RunTestAsync(async () =>
        {
            var consultation = await GetFirstConsultation();
            var room = await GetFirstRoom();
            var user = await GetFirstUser();

            var request = new
            {
                consultationId = consultation.Id,
                userId = user.Id,
                comment = "Test registration",
                roomId = room.Id,
            };

            var response = await _client.PostAsJsonAsync($"{BaseUrl}/register", request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            Assert.True(root.TryGetProperty("userId", out _), "Response should have 'userId' field");
            Assert.True(root.TryGetProperty("firstName", out _), "Response should have 'firstName' field");
            Assert.True(root.TryGetProperty("lastName", out _), "Response should have 'lastName' field");
            Assert.True(root.TryGetProperty("status", out _), "Response should have 'status' field");
            Assert.True(root.TryGetProperty("comment", out _), "Response should have 'comment' field");
        });
    }

    [LoggedFact(Category = "AttendanceController", Points = 5)]
    public async Task Register_ShouldIncrementConsultationStudentCount()
    {
        await RunTestAsync(async () =>
        {
            var consultation = await GetFirstConsultation();
            var user = await GetFirstUser();
            var room = await GetFirstRoom();
            var initialStudents = consultation.RegisteredStudents;

            var request = new
            {
                consultationId = consultation.Id,
                userId = user.Id,
                roomId = room.Id,
                comment = "Increment test"
            };

            await _client.PostAsJsonAsync($"{BaseUrl}/register", request);

            var updated = TestDatabaseHelper.GetById<Consultation>(_factory.Services, c => c.Id == consultation.Id);
            Assert.NotNull(updated);
            Assert.Equal(initialStudents + 1, updated.RegisteredStudents);
        });
    }

    [LoggedFact(Category = "AttendanceController", Points = 10)]
    public async Task Delete_ValidId_ShouldReturn200()
    {
        await RunTestAsync(async () =>
        {
            var attendanceToDelete = await GetFirstAttendanceToDelete();

            var response = await _client.DeleteAsync($"{BaseUrl}/{attendanceToDelete.Id}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        });
    }

    [LoggedFact(Category = "AttendanceController", Points = 10)]
    public async Task GetByConsultation_ShouldReturnJsonArrayWithExpectedFields()
    {
        await RunTestAsync(async () =>
        {
            Consultation consultation;
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<Repository.ApplicationDbContext>();
                consultation = await context.Consultations
                    .Include(c => c.Attendances)
                    .FirstAsync(c => c.Attendances.Any());
            }

            var response = await _client.GetAsync($"{BaseUrl}/consultation/{consultation.Id}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            Assert.Equal(JsonValueKind.Array, root.ValueKind);
            Assert.True(root.GetArrayLength() > 0, "Should have at least one attendance");

            var first = root[0];
            Assert.True(first.TryGetProperty("userId", out _), "Response should have 'userId' field");
            Assert.True(first.TryGetProperty("firstName", out _), "Response should have 'firstName' field");
            Assert.True(first.TryGetProperty("lastName", out _), "Response should have 'lastName' field");
            Assert.True(first.TryGetProperty("status", out _), "Response should have 'status' field");
            Assert.True(first.TryGetProperty("comment", out _), "Response should have 'comment' field");
        });
    }

    [LoggedFact(Category = "AttendanceController", Points = 5)]
    public async Task GetByConsultation_EmptyConsultation_ShouldReturnEmptyArray()
    {
        await RunTestAsync(async () =>
        {
            var room = await TestDatabaseHelper.GetFirst<Room>(_factory.Services);
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<Repository.ApplicationDbContext>();
            var consultation = new Consultation
            {
                Id = Guid.NewGuid(),
                StartTime = DateTime.UtcNow.AddDays(50),
                EndTime = DateTime.UtcNow.AddDays(50).AddHours(1),
                RoomId = room.Id,
                RegisteredStudents = 0,
                CreatedAt = DateTime.UtcNow,
                CreatedById = "test-user-1",
                LastModifiedAt = DateTime.UtcNow,
                LastModifiedById = "test-user-1"
            };
            context.Consultations.Add(consultation);
            await context.SaveChangesAsync();

            var response = await _client.GetAsync($"{BaseUrl}/consultation/{consultation.Id}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            Assert.Equal(JsonValueKind.Array, doc.RootElement.ValueKind);
            Assert.Equal(0, doc.RootElement.GetArrayLength());
        });
    }

    [LoggedFact(Category = "AttendanceController", Points = 10)]
    public async Task MarkAsAbsent_ShouldReturn200()
    {
        await RunTestAsync(async () =>
        {
            var attendance = await GetFirstAttendance();

            var response = await _client.PatchAsync($"{BaseUrl}/{attendance.Id}/mark-as-absent", null);
            response.EnsureSuccessStatusCode();
        });
    }

    [LoggedFact(Category = "AttendanceController", Points = 10)]
    public async Task UploadCancelationReason_ShouldReturn200()
    {
        await RunTestAsync(async () =>
        {
            var attendance = await GetFirstAttendance();

            using var content = new MultipartFormDataContent();
            var fileBytes = "test file content"u8.ToArray();
            var fileContent = new ByteArrayContent(fileBytes);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
            content.Add(fileContent, "file", "test_reason.pdf");

            var response = await _client.PostAsync($"{BaseUrl}/{attendance.Id}/cancelation-reason", content);
            response.EnsureSuccessStatusCode();
        });
    }

    [LoggedFact(Category = "AttendanceController", Points = 5)]
    public async Task Delete_InvalidId_ShouldReturnError()
    {
        await RunTestAsync(async () =>
        {
            var response = await _client.DeleteAsync($"{BaseUrl}/{Guid.NewGuid()}");
            Assert.True(response.StatusCode == HttpStatusCode.InternalServerError ||
                        response.StatusCode == HttpStatusCode.NotFound ||
                        response.StatusCode == HttpStatusCode.BadRequest,
                $"Expected error status but got {response.StatusCode}");
        });
    }

    private async Task<Consultation> GetFirstConsultation()
    {
        return await TestDatabaseHelper.GetFirst<Consultation>(_factory.Services);
    }    
    
    private async Task<Room> GetFirstRoom()
    {
        return await TestDatabaseHelper.GetFirst<Room>(_factory.Services);
    }

    private async Task<ConsultationsApplicationUser> GetFirstUser()
    {
        return await TestDatabaseHelper.GetFirst<ConsultationsApplicationUser>(_factory.Services);
    }
    
    private async Task<Attendance> GetFirstAttendanceToDelete()
    {
        return await TestDatabaseHelper.GetFirstWhere<Attendance>(
            _factory.Services,
            predicate: x =>
                x.Consultation.RegisteredStudents == 0 &&
                x.Consultation.StartTime > DateTime.Now.AddHours(1)
        );
    }

    private async Task<Attendance> GetFirstAttendance()
    {
        return await TestDatabaseHelper.GetFirst<Attendance>(_factory.Services);
    }
}
