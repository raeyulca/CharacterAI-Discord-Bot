using Newtonsoft.Json;

namespace CharacterAI_Discord_Bot.Models.Response;

public class HistoryResponse
{
    [JsonProperty("status")]
    public string? Status { get; set; }

    [JsonProperty("external_id")]
    public string? ExternalId { get; set; }
}