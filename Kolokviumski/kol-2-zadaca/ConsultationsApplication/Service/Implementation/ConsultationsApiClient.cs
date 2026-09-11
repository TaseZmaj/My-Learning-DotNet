using System.Net.Http.Json;
using Domain.Configuration;
using Domain.Dto;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Service.Interface;

namespace Service.Implementation;

public class ConsultationsApiClient : IConsultationsApiClient<ExternalConsultationsDto>
{
    private readonly HttpClient _httpClient;
    private readonly ConsultationsApiSettings _settings;
    
    public ConsultationsApiClient(
        HttpClient httpClient,
        IOptions<ConsultationsApiSettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
    }

    public async Task<ExternalConsultationsDto> GetAllConsultationsModifiedSinceAsync(DateTime dateLastModified)
    {
        var apiKey = _settings.ApiKey;
        _httpClient.DefaultRequestHeaders.Add("X-API-KEY", apiKey);
        
        var url = $"/api/external/consultations?modifiedSince={dateLastModified}";

        var response = await _httpClient.GetAsync(url);
        
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<ExternalConsultationsDto>();
    }
}