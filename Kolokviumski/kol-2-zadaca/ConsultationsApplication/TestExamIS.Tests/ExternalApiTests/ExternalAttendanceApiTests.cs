using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Domain.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using TestExamIS.Tests.Utils;

namespace TestExamIS.Tests.ExternalApiTests;

[Collection("Test Suite")]
public class ExternalAttendanceApiTests : LoggedTestBase, IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _anonymousClient;
    private const string BaseUrl = "/api/external/attendance/register";
    private const string DevApiKey = "HyeORCiubWiUO4E1m1h3dGPjPKWhND1f";

    public ExternalAttendanceApiTests(WebApplicationFactory<Program> factory, GlobalTestFixture fixture) : base(fixture)
    {
        _factory = factory
            .WithTestDatabase()
            .WithWebHostBuilder(b => b.ConfigureAppConfiguration((_, cfg) =>
                cfg.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ApiKeySettings:ApiKey"] = DevApiKey
                })));

        _anonymousClient = _factory.CreateAnonymousClient();
        _anonymousClient.Timeout = TimeSpan.FromSeconds(5);
    }

    public async Task InitializeAsync() => await TestDatabaseHelper.ResetDatabaseAsync(_factory.Services);
    public Task DisposeAsync() => Task.CompletedTask;

    private HttpClient CreateClientWithKey(string apiKey)
    {
        var client = _factory.CreateAnonymousClient();
        client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
        client.Timeout = TimeSpan.FromSeconds(5);
        return client;
    }

    private async Task<object> BuildPayloadAsync()
    {
        var consultation = await TestDatabaseHelper.GetFirst<Consultation>(_factory.Services);
        var room = await TestDatabaseHelper.GetFirst<Room>(_factory.Services);
        return new { consultationId = consultation.Id, userId = "test-user-1", roomId = room.Id };
    }

    [LoggedFact(Category = "ExternalApi", Points = 5)]
    public async Task Register_MissingApiKey_ShouldReturn401WithMessage()
    {
        await RunTestAsync(async () =>
        {
            var payload = await BuildPayloadAsync();
            var response = await _anonymousClient.PostAsJsonAsync(BaseUrl, payload);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            Assert.Contains("API key is required", body, StringComparison.OrdinalIgnoreCase);
        });
    }

    [LoggedFact(Category = "ExternalApi", Points = 5)]
    public async Task Register_InvalidApiKey_ShouldReturn401WithMessage()
    {
        await RunTestAsync(async () =>
        {
            var client = CreateClientWithKey("totally-invalid-key-xyz");
            var payload = await BuildPayloadAsync();
            var response = await client.PostAsJsonAsync(BaseUrl, payload);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            Assert.Contains("Invalid API key", body, StringComparison.OrdinalIgnoreCase);
        });
    }

    [LoggedFact(Category = "ExternalApi", Points = 8)]
    public async Task Register_ValidApiKey_ShouldReturn202Accepted()
    {
        await RunTestAsync(async () =>
        {
            var client = CreateClientWithKey(DevApiKey);
            var payload = await BuildPayloadAsync();
            var response = await client.PostAsJsonAsync(BaseUrl, payload);

            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        });
    }

    [LoggedFact(Category = "ExternalApi", Points = 8)]
    public async Task Register_ValidApiKey_ShouldCreateInboundEventEntry()
    {
        await RunTestAsync(async () =>
        {
            var countBefore = await TestDatabaseHelper.GetCount<InboundEventEntry>(_factory.Services);

            var client = CreateClientWithKey(DevApiKey);
            var payload = await BuildPayloadAsync();
            var response = await client.PostAsJsonAsync(BaseUrl, payload);
            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);

            var countAfter = await TestDatabaseHelper.GetCount<InboundEventEntry>(_factory.Services);
            Assert.Equal(countBefore + 1, countAfter);
        });
    }

    [LoggedFact(Category = "ExternalApi", Points = 5)]
    public async Task GetStatus_ValidId_ShouldReturnStatusObject()
    {
        await RunTestAsync(async () =>
        {
            var client = CreateClientWithKey(DevApiKey);
            var payload = await BuildPayloadAsync();

            var postResponse = await client.PostAsJsonAsync(BaseUrl, payload);
            Assert.Equal(HttpStatusCode.Accepted, postResponse.StatusCode);

            var postBody = await postResponse.Content.ReadAsStringAsync();
            using var postDoc = JsonDocument.Parse(postBody);
            Assert.True(postDoc.RootElement.TryGetProperty("id", out var idProp),
                "202 response must contain 'id' field");
            var id = idProp.GetString();
            Assert.False(string.IsNullOrEmpty(id), "'id' in 202 response must not be empty");

            var statusResponse = await client.GetAsync($"{BaseUrl}/{id}/status");
            Assert.Equal(HttpStatusCode.OK, statusResponse.StatusCode);

            var statusBody = await statusResponse.Content.ReadAsStringAsync();
            using var statusDoc = JsonDocument.Parse(statusBody);
            Assert.True(statusDoc.RootElement.TryGetProperty("status", out _),
                "Status endpoint response must contain 'status' field");
            Assert.True(statusDoc.RootElement.TryGetProperty("id", out _),
                "Status endpoint response must contain 'id' field");
        });
    }

    [LoggedFact(Category = "ExternalApi", Points = 3)]
    public async Task GetStatus_InvalidId_ShouldReturnError()
    {
        await RunTestAsync(async () =>
        {
            var client = CreateClientWithKey(DevApiKey);
            var response = await client.GetAsync($"{BaseUrl}/{Guid.NewGuid()}/status");

            Assert.True(
                response.StatusCode == HttpStatusCode.NotFound ||
                response.StatusCode == HttpStatusCode.BadRequest ||
                response.StatusCode == HttpStatusCode.InternalServerError,
                $"Expected error status for invalid id, got {response.StatusCode}");
        });
    }
}
