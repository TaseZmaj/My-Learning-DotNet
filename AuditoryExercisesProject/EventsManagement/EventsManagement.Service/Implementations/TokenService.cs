using EventsManagement.Domain.Configuration;
using EventsManagement.Domain.ExternalModels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace Service.Implementations;

//OAuth2 token service
public class TokenService
{
    private readonly HttpClient _http;
    private readonly ExternalEventApiSettings _settings;
    private readonly ILogger<TokenService> _logger;

    private string? _cachedToken;
    private DateTime _tokenExpiresAt = DateTime.MinValue;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public TokenService(
        HttpClient http,
        IOptions<ExternalEventApiSettings> options,
        ILogger<TokenService> logger)
    {
        _http = http;
        _settings = options.Value;
        _logger = logger;
    }

    public async Task<string> GetTokenAsync(CancellationToken ct)
    {
        // Return cached token if still valid (with a 30-second buffer)
        if (_cachedToken is not null && DateTime.UtcNow < _tokenExpiresAt.AddSeconds(-30))
        {
            return _cachedToken;
        }

        // Acquire lock so only one thread refreshes at a time
        //Thread Safety: The SemaphoreSlim ensures that if 10
        //users trigger an event at the exact same millisecond,
        //your app only sends one request for a new token instead of 10.
        await _lock.WaitAsync(ct);
        try
        {
            // Second check after lock to see if another thread refreshed it
            if (_cachedToken is not null && DateTime.UtcNow < _tokenExpiresAt.AddSeconds(-30))
            {
                return _cachedToken;
            }

            _logger.LogInformation("Requesting new OAuth2 token");

            var request = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = _settings.ClientId,
                ["client_secret"] = _settings.ClientSecret,
                ["scope"] = _settings.Scope
            });

            var response = await _http.PostAsync(_settings.TokenUrl, request, ct);
            response.EnsureSuccessStatusCode();

            var token = await response.Content.ReadFromJsonAsync<TokenResponse>(ct);

            _cachedToken = token!.AccessToken;
            _tokenExpiresAt = DateTime.UtcNow.AddSeconds(token.ExpiresIn);

            _logger.LogInformation("Token acquired, expires at {ExpiresAt}", _tokenExpiresAt);

            return _cachedToken;
        }
        finally
        {
            _lock.Release();
        }
    }
}