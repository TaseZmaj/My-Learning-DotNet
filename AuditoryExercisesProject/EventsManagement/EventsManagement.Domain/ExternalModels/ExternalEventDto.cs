namespace EventsManagement.Domain.ExternalModels;

using System.Text.Json.Serialization;


public class ExternalEventDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("location")]
    public string Location { get; set; } = string.Empty;

    [JsonPropertyName("start_date")]
    public DateTime StartDate { get; set; }

    [JsonPropertyName("end_date")]
    public DateTime EndDate { get; set; }

    [JsonPropertyName("available_tickets")]
    public int AvailableTickets { get; set; }

    [JsonPropertyName("price")]
    public decimal Price { get; set; }
}