using System.Text.Json.Serialization;

namespace EventsManagement.Domain.ExternalModels;


//This class is used for the OAuth2 TokenService
public class TokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;
}