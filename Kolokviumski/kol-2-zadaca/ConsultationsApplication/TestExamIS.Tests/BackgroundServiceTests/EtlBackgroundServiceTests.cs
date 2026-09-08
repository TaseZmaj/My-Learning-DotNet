using Domain.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Service.Interface;
using TestExamIS.Tests.Utils;

namespace TestExamIS.Tests.BackgroundServiceTests;

[Collection("Test Suite")]
public class EtlBackgroundServiceTests : LoggedTestBase, IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly TestEtlSyncService _etlService = new();

    public EtlBackgroundServiceTests(WebApplicationFactory<Program> factory, GlobalTestFixture fixture) : base(fixture)
    {
        _factory = factory
            .WithTestDatabase()
            .WithWebHostBuilder(builder =>
                builder.ConfigureTestServices(services =>
                {
                    // Replace real ETL service so background service doesn't hit the external API
                    services.AddScoped<IEtlSyncService>(_ => _etlService);
                }));
    }

    public async Task InitializeAsync() =>
        await TestDatabaseHelper.ResetDatabaseAsync(_factory.Services);

    public Task DisposeAsync() => Task.CompletedTask;

    [LoggedFact(Category = "BackgroundService", Points = 5)]
    public async Task EtlSync_BackgroundService_ShouldBeRegisteredAsHostedService()
    {
        await RunTestAsync(async () =>
        {
            var hostedServices = _factory.Services.GetServices<IHostedService>();
            var hasEtlBackgroundService = hostedServices
                .Any(s => s.GetType().IsAssignableTo(typeof(BackgroundService))
                          && s.GetType().Name.Contains("Sync", StringComparison.OrdinalIgnoreCase));

            Assert.True(hasEtlBackgroundService,
                "A BackgroundService for ETL sync must be registered as IHostedService");
        });
    }

    [LoggedFact(Category = "BackgroundService", Points = 8)]
    public async Task EtlSync_BackgroundService_ShouldCallSyncAllAsyncOnStartup()
    {
        await RunTestAsync(async () =>
        {
            // Background service fires immediately on host start (before first 5-min delay)
            // Host starts when _factory.Services is first accessed (in InitializeAsync)
            // Give it a moment to complete the first run
            await Task.Delay(500);

            Assert.True(_etlService.CallCount > 0,
                $"BackgroundService must call IEtlSyncService.SyncAllAsync() at least once on startup. " +
                $"CallCount was {_etlService.CallCount}");
        });
    }

    // ── Test double ──────────────────────────────────────────────────────────

    private sealed class TestEtlSyncService : IEtlSyncService
    {
        private volatile int _callCount;
        internal int CallCount => _callCount;

        public Task SyncAllAsync()
        {
            Interlocked.Increment(ref _callCount);
            return Task.CompletedTask;
        }
    }
}
