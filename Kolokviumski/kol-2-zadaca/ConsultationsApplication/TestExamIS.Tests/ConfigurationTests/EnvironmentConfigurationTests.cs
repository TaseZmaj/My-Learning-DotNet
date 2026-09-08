using Microsoft.Extensions.Configuration;
using TestExamIS.Tests.Utils;

namespace TestExamIS.Tests.ConfigurationTests;

[Collection("Test Suite")]
public class EnvironmentConfigurationTests : LoggedTestBase
{
    public EnvironmentConfigurationTests(GlobalTestFixture fixture) : base(fixture) { }

    private static IConfiguration BuildConfig(string env) =>
        new ConfigurationBuilder()
            .SetBasePath(Path.GetDirectoryName(typeof(Program).Assembly.Location)!)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{env}.json", optional: true)
            .Build();

    [LoggedFact(Category = "Configuration", Points = 3)]
    public async Task Development_CacheSettings_ShouldHaveCorrectValues()
    {
        await RunTestAsync(async () =>
        {
            var config = BuildConfig("Development");
            Assert.Equal("10", config["CacheSettings:ListCacheDurationMinutes"]);
            Assert.Equal("15", config["CacheSettings:DetailCacheDurationMinutes"]);
            await Task.CompletedTask;
        });
    }

    [LoggedFact(Category = "Configuration", Points = 3)]
    public async Task Staging_CacheSettings_ShouldHaveCorrectValues()
    {
        await RunTestAsync(async () =>
        {
            var config = BuildConfig("Staging");
            Assert.Equal("20", config["CacheSettings:ListCacheDurationMinutes"]);
            Assert.Equal("25", config["CacheSettings:DetailCacheDurationMinutes"]);
            await Task.CompletedTask;
        });
    }

    [LoggedFact(Category = "Configuration", Points = 3)]
    public async Task Production_CacheSettings_ShouldHaveCorrectValues()
    {
        await RunTestAsync(async () =>
        {
            var config = BuildConfig("Production");
            Assert.Equal("60", config["CacheSettings:ListCacheDurationMinutes"]);
            Assert.Equal("60", config["CacheSettings:DetailCacheDurationMinutes"]);
            await Task.CompletedTask;
        });
    }

    [LoggedFact(Category = "Configuration", Points = 3)]
    public async Task Development_ApiKeySettings_ShouldHaveCorrectKey()
    {
        await RunTestAsync(async () =>
        {
            var config = BuildConfig("Development");
            Assert.Equal("HyeORCiubWiUO4E1m1h3dGPjPKWhND1f", config["ApiKeySettings:ApiKey"]);
            await Task.CompletedTask;
        });
    }

    [LoggedFact(Category = "Configuration", Points = 3)]
    public async Task Staging_ApiKeySettings_ShouldHaveCorrectKey()
    {
        await RunTestAsync(async () =>
        {
            var config = BuildConfig("Staging");
            Assert.Equal("rVRF58bzXD00bxIhQin2NCozkapmVRQy", config["ApiKeySettings:ApiKey"]);
            await Task.CompletedTask;
        });
    }

    [LoggedFact(Category = "Configuration", Points = 3)]
    public async Task Production_ApiKeySettings_ShouldHaveCorrectKey()
    {
        await RunTestAsync(async () =>
        {
            var config = BuildConfig("Production");
            Assert.Equal("Cnp8tzHRbwNQCgrBadTLBtnRvZtBcDYC", config["ApiKeySettings:ApiKey"]);
            await Task.CompletedTask;
        });
    }

    [LoggedFact(Category = "Configuration", Points = 3)]
    public async Task RateLimitSettings_ShouldHavePositiveValues_ForAllEnvironments()
    {
        await RunTestAsync(async () =>
        {
            foreach (var env in new[] { "Development", "Staging", "Production" })
            {
                var config = BuildConfig(env);

                var permitLimit = config["RateLimitSettings:PermitLimit"];
                Assert.True(
                    int.TryParse(permitLimit, out var pl) && pl > 0,
                    $"[{env}] RateLimitSettings:PermitLimit must be a positive integer, got: '{permitLimit}'");

                var windowInSeconds = config["RateLimitSettings:WindowInSeconds"];
                Assert.True(
                    int.TryParse(windowInSeconds, out var ws) && ws > 0,
                    $"[{env}] RateLimitSettings:WindowInSeconds must be a positive integer, got: '{windowInSeconds}'");
            }
            await Task.CompletedTask;
        });
    }
}
