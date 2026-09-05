using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Domain.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using TestExamIS.Tests.Utils;

namespace TestExamIS.Tests.ControllersTests;

[Collection("Test Suite")]
public class ConsultationControllerTests : LoggedTestBase, IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private const string ControllerName = "ConsultationController";
    private const string BaseUrl = "/api/Consultation";

    public ConsultationControllerTests(WebApplicationFactory<Program> factory, GlobalTestFixture fixture) : base(fixture)
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

    [LoggedFact(Category = "ConsultationController", Points = 8)]
    public async Task GetAll_ShouldReturnJsonArrayWithExpectedFields()
    {
        await RunTestAsync(async () =>
        {
            var response = await _client.GetAsync(BaseUrl);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            Assert.Equal(JsonValueKind.Array, root.ValueKind);
            Assert.True(root.GetArrayLength() > 0, "Should return at least one consultation");

            var first = root[0];
            Assert.True(first.TryGetProperty("id", out _), "Response should have 'id' field");
            Assert.True(first.TryGetProperty("date", out _), "Response should have 'date' field");
            Assert.True(first.TryGetProperty("roomId", out _), "Response should have 'roomId' field");
            Assert.True(first.TryGetProperty("registeredStudents", out _), "Response should have 'registeredStudents' field");
        });
    }

    [LoggedFact(Category = "ConsultationController", Points = 7)]
    public async Task GetAll_WithRoomFilter_ShouldFilterResults()
    {
        await RunTestAsync(async () =>
        {
            var response = await _client.GetAsync($"{BaseUrl}?roomNumber=A101");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            Assert.Equal(JsonValueKind.Array, root.ValueKind);
            // Filtered results should be fewer or equal to all results
            var allResponse = await _client.GetAsync(BaseUrl);
            var allJson = await allResponse.Content.ReadAsStringAsync();
            using var allDoc = JsonDocument.Parse(allJson);
            Assert.True(root.GetArrayLength() <= allDoc.RootElement.GetArrayLength());
        });
    }

    [LoggedFact(Category = "ConsultationController", Points = 10)]
    public async Task Register_ValidRequest_ShouldCreateAndReturnFields()
    {
        await RunTestAsync(async () =>
        {
            var room = await TestDatabaseHelper.GetFirst<Room>(_factory.Services);

            var request = new
            {
                roomId = room.Id,
                startTime = DateTime.UtcNow.AddDays(30).ToString("o"),
                endTime = DateTime.UtcNow.AddDays(30).AddHours(2).ToString("o")
            };

            var response = await _client.PostAsJsonAsync($"{BaseUrl}", request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            Assert.True(root.TryGetProperty("id", out var idProp), "Response should have 'id' field");
            Assert.NotEqual(Guid.Empty.ToString(), idProp.GetString());
            Assert.True(root.TryGetProperty("start", out _), "Response should have 'start' field");
            Assert.True(root.TryGetProperty("end", out _), "Response should have 'end' field");
            Assert.True(root.TryGetProperty("roomId", out var roomIdProp), "Response should have 'roomId' field");
            Assert.Equal(room.Id.ToString(), roomIdProp.GetString());
        });
    }

    [LoggedFact(Category = "ConsultationController", Points = 5)]
    public async Task Register_ShouldIncrementCount()
    {
        await RunTestAsync(async () =>
        {
            var initialCount = await TestDatabaseHelper.GetCount<Consultation>(_factory.Services);
            var room = await TestDatabaseHelper.GetFirst<Room>(_factory.Services);

            var request = new
            {
                roomId = room.Id,
                startTime = DateTime.UtcNow.AddDays(31),
                endTime = DateTime.UtcNow.AddDays(31).AddHours(2)
            };

            var response = await _client.PostAsJsonAsync($"{BaseUrl}", request);
            response.EnsureSuccessStatusCode();

            var newCount = await TestDatabaseHelper.GetCount<Consultation>(_factory.Services);
            Assert.Equal(initialCount + 1, newCount);
        });
    }

    [LoggedFact(Category = "ConsultationController", Points = 10)]
    public async Task Update_ValidRequest_ShouldUpdateAndReturnFields()
    {
        await RunTestAsync(async () =>
        {
            var consultation = await TestDatabaseHelper.GetFirst<Consultation>(_factory.Services);
            var room = await TestDatabaseHelper.GetFirst<Room>(_factory.Services);

            var newStart = DateTime.UtcNow.AddDays(40);
            var request = new
            {
                roomId = room.Id,
                startTime = newStart,
                endTime = newStart.AddHours(3)
            };

            var response = await _client.PutAsJsonAsync($"{BaseUrl}/{consultation.Id}", request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            Assert.True(root.TryGetProperty("id", out _), "Response should have 'id' field");
            Assert.True(root.TryGetProperty("roomId", out var roomIdProp), "Response should have 'roomId' field");
            Assert.Equal(room.Id.ToString(), roomIdProp.GetString());
        });
    }

    [LoggedFact(Category = "ConsultationController", Points = 5)]
    public async Task Update_InvalidId_ShouldReturnError()
    {
        await RunTestAsync(async () =>
        {
            var room = await TestDatabaseHelper.GetFirst<Room>(_factory.Services);
            var request = new
            {
                roomId = room.Id,
                startTime = DateTime.UtcNow.AddDays(1),
                endTime = DateTime.UtcNow.AddDays(1).AddHours(1)
            };

            var response = await _client.PutAsJsonAsync($"{BaseUrl}/{Guid.NewGuid()}", request);
            Assert.True(response.StatusCode == HttpStatusCode.InternalServerError ||
                        response.StatusCode == HttpStatusCode.NotFound ||
                        response.StatusCode == HttpStatusCode.BadRequest,
                $"Expected error status but got {response.StatusCode}");
        });
    }

    [LoggedFact(Category = "ConsultationController", Points = 10)]
    public async Task Delete_ValidId_ShouldDeleteAndReturn()
    {
        await RunTestAsync(async () =>
        {
            var initialCount = await TestDatabaseHelper.GetCount<Consultation>(_factory.Services);
            var consultation = await TestDatabaseHelper.GetFirst<Consultation>(_factory.Services);

            var response = await _client.DeleteAsync($"{BaseUrl}/{consultation.Id}");
            response.EnsureSuccessStatusCode();

            var newCount = await TestDatabaseHelper.GetCount<Consultation>(_factory.Services);
            Assert.Equal(initialCount - 1, newCount);
        });
    }

    [LoggedFact(Category = "ConsultationController", Points = 5)]
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

    [LoggedFact(Category = "ConsultationController", Points = 10)]
    public async Task GetPaged_ShouldReturnPaginatedResponse()
    {
        await RunTestAsync(async () =>
        {
            var pageSize = 5;
            var response = await _client.GetAsync($"{BaseUrl}/paged?pageNumber=0&pageSize={pageSize}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            Assert.True(root.TryGetProperty("items", out var items), "Response should have 'items' field");
            Assert.Equal(JsonValueKind.Array, items.ValueKind);
            Assert.True(items.GetArrayLength() <= pageSize, $"Items count should be at most {pageSize}");
            Assert.True(items.GetArrayLength() > 0, "Items should not be empty");

            Assert.True(root.TryGetProperty("totalCount", out var totalCount), "Response should have 'totalCount' field");
            Assert.True(totalCount.GetInt32() >= items.GetArrayLength(), "Total count should be >= returned items");

            Assert.True(root.TryGetProperty("pageNumber", out var pageNumber), "Response should have 'pageNumber' field");
            Assert.Equal(0, pageNumber.GetInt32());

            Assert.True(root.TryGetProperty("pageSize", out var pageSizeProp), "Response should have 'pageSize' field");
            Assert.Equal(pageSize, pageSizeProp.GetInt32());

            Assert.True(root.TryGetProperty("totalPages", out _), "Response should have 'totalPages' field");

            var first = items[0];
            Assert.True(first.TryGetProperty("id", out _), "Item should have 'id' field");
            Assert.True(first.TryGetProperty("date", out _), "Item should have 'date' field");
            Assert.True(first.TryGetProperty("roomId", out _), "Item should have 'roomId' field");
            Assert.True(first.TryGetProperty("registeredStudents", out _), "Item should have 'registeredStudents' field");
            Assert.True(first.TryGetProperty("attendances", out _), "Item should have 'attendances' field");
        });
    }
}
