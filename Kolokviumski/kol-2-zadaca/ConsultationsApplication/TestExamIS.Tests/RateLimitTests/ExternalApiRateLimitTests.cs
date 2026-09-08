using System.Net;
using System.Net.Http.Json;
using Domain.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using TestExamIS.Tests.Utils;

namespace TestExamIS.Tests.RateLimitTests;

[Collection("Test Suite")]
public class ExternalApiRateLimitTests : LoggedTestBase, IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private const string BaseUrl = "/api/external/attendance/register";
    private const string DevApiKey = "HyeORCiubWiUO4E1m1h3dGPjPKWhND1f";

    public ExternalApiRateLimitTests(WebApplicationFactory<Program> factory, GlobalTestFixture fixture) : base(fixture)
    {
        _factory = factory
            .WithTestDatabase()
            .WithWebHostBuilder(b => b.ConfigureAppConfiguration((_, cfg) =>
                cfg.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ApiKeySettings:ApiKey"] = DevApiKey,
                    ["RateLimitSettings:PermitLimit"] = "2",
                    ["RateLimitSettings:WindowInSeconds"] = "60"
                })));
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

    [LoggedFact(Category = "RateLimit", Points = 5)]
    public async Task ExternalApi_WithinLimit_ShouldSucceed()
    {
        await RunTestAsync(async () =>
        {
            var client = CreateClientWithKey(DevApiKey);
            var payload = await BuildPayloadAsync();

            var r1 = await client.PostAsJsonAsync(BaseUrl, payload);
            var r2 = await client.PostAsJsonAsync(BaseUrl, payload);

            Assert.Equal(HttpStatusCode.Accepted, r1.StatusCode);
            Assert.Equal(HttpStatusCode.Accepted, r2.StatusCode);
        });
    }

    [LoggedFact(Category = "RateLimit", Points = 8)]
    public async Task ExternalApi_ExceedingLimit_ShouldReturn429()
    {
        await RunTestAsync(async () =>
        {
            var client = CreateClientWithKey(DevApiKey);
            var payload = await BuildPayloadAsync();

            var r1 = await client.PostAsJsonAsync(BaseUrl, payload);
            var r2 = await client.PostAsJsonAsync(BaseUrl, payload);
            Assert.Equal(HttpStatusCode.Accepted, r1.StatusCode);
            Assert.Equal(HttpStatusCode.Accepted, r2.StatusCode);

            var r3 = await client.PostAsJsonAsync(BaseUrl, payload);
            Assert.Equal(HttpStatusCode.TooManyRequests, r3.StatusCode);
        });
    }
}
