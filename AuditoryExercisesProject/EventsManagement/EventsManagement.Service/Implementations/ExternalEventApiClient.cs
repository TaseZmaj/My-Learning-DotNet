using EventsManagement.Domain.ExternalModels;

namespace Service.Implementations;

using Interfaces;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

public class ExternalEventApiClient : IExternalEventApiClient
{
    private readonly HttpClient _http;
    private readonly TokenService _tokenService;
    private readonly ILogger<ExternalEventApiClient> _logger;

    public ExternalEventApiClient(
        HttpClient http,
        TokenService tokenService,
        ILogger<ExternalEventApiClient> logger)
    {
        _http = http;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<List<ExternalEventDto>> GetEventsAsync(CancellationToken ct)
    {
        // 1. Get a valid token (The Client doesn't care if it's cached or new)
        var token = await _tokenService.GetTokenAsync(ct);

        // 2. Attach the token to the Authorization header
        _http.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);

        // 3. Make the actual data request
        var response = await _http.GetAsync("api/events", ct);

        // 4. Handle specific auth failures
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            _logger.LogWarning("Token rejected (401), may need refresh");
        }

        response.EnsureSuccessStatusCode();

        // 5. Return the list of events (or an empty list if null)
        return await response.Content.ReadFromJsonAsync<List<ExternalEventDto>>(ct) 
               ?? [];
    }
}