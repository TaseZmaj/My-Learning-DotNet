using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TestExamIS.Tests.Utils;

namespace TestExamIS.Tests.CacheTests;

[Collection("Test Suite")]
public class ConsultationCacheTests : LoggedTestBase, IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly IMemoryCache _memoryCache;

    public ConsultationCacheTests(WebApplicationFactory<Program> factory, GlobalTestFixture fixture) : base(fixture)
    {
        _factory = factory.WithTestDatabase().WithTestAuth();
        _client = _factory.CreateAuthenticatedClient();
        _client.Timeout = TimeSpan.FromSeconds(5);
        _memoryCache = _factory.Services.GetRequiredService<IMemoryCache>();
    }

    public async Task InitializeAsync() => await TestDatabaseHelper.ResetDatabaseAsync(_factory.Services);
    public Task DisposeAsync() => Task.CompletedTask;

    [LoggedFact(Category = "Cache", Points = 5)]
    public async Task GetAll_WithRoomAndDateFilter_ShouldPopulateMemoryCache()
    {
        await RunTestAsync(async () =>
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var url = $"/api/Consultation?roomName=A101&date={today:yyyy-MM-dd}";

            var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var cacheKey = $"consultations:A101:{today:yyyy-MM-dd}";
            Assert.True(
                _memoryCache.TryGetValue(cacheKey, out _),
                $"Expected IMemoryCache to contain key '{cacheKey}' after GET {url}");
        });
    }

    [LoggedFact(Category = "Cache", Points = 5)]
    public async Task GetAll_NoFilter_ShouldPopulateMemoryCacheWithEmptyKey()
    {
        await RunTestAsync(async () =>
        {
            var response = await _client.GetAsync("/api/Consultation");
            response.EnsureSuccessStatusCode();

            var cacheKey = "consultations::";
            Assert.True(
                _memoryCache.TryGetValue(cacheKey, out _),
                $"Expected IMemoryCache to contain key '{cacheKey}' after GET /api/Consultation with no filters");
        });
    }

    [LoggedFact(Category = "Cache", Points = 5)]
    public async Task GetAll_CacheDuration_ShouldComeFromConfiguration()
    {
        await RunTestAsync(async () =>
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Path.GetDirectoryName(typeof(Program).Assembly.Location)!)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            var listDuration = config["CacheSettings:ListCacheDurationMinutes"];
            Assert.NotNull(listDuration);
            Assert.True(
                int.TryParse(listDuration, out var dur) && dur > 0,
                $"CacheSettings:ListCacheDurationMinutes must be a positive integer in Development config, got: '{listDuration}'");

            var response = await _client.GetAsync("/api/Consultation");
            response.EnsureSuccessStatusCode();

            Assert.True(
                _memoryCache.TryGetValue("consultations::", out _),
                "Cache must be populated using duration from CacheSettings:ListCacheDurationMinutes");
        });
    }
}
