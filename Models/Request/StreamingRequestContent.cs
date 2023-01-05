using Newtonsoft.Json;

namespace CharacterAI_Discord_Bot.Models.Request;

public class StreamingRequestContent
{
    public StreamingRequestContent()
    {

    }

    public StreamingRequestContent(Character charInfo, string msg, string imgPath)
    {
        if (!string.IsNullOrEmpty(imgPath))
        {
            ImageDescriptionType = "AUTO_IMAGE_CAPTIONING";
            ImageOriginType = "UPLOADED";
            ImageRelPath = imgPath;
        }

        CharacterExternalId = charInfo.CharId!;
        EnableTTI = true;
        HistoryExternalId = charInfo.HistoryExternalId!;
        Text = msg;
        Tgt = charInfo.Tgt!;
        RankingMethod = "random";
        Staging = false;
        StreamEveryNSteps = 16;
        ChunksToPad = 8;
        IsProactive = false;

    }
    [JsonProperty("character_external_id")]
    public string CharacterExternalId { get; set; }
    
    [JsonProperty("enable_tti")]
    public bool EnableTTI { get; set; }
    
    [JsonProperty("history_external_id")]
    public string HistoryExternalId { get; set; }
    
    [JsonProperty("text")]
    public string Text { get; set; }
    
    [JsonProperty("tgt")]
    public string Tgt { get; set; }
    
    [JsonProperty("ranking_method")]
    public string RankingMethod { get; set; }

    [JsonProperty("staging")]
    public bool Staging { get; set; }

    [JsonProperty("stream_every_n_steps")]
    public int StreamEveryNSteps { get; set; }

    [JsonProperty("chunks_to_pad")]
    public int ChunksToPad { get; set; }
    
    [JsonProperty("is_proactive")]
    public bool IsProactive { get; set; }

    [JsonProperty("image_description_type")]
    public string ImageDescriptionType { get; set; }

    [JsonProperty("ImageOriginType")]
    public string ImageOriginType { get; set;}

    [JsonProperty("image_rel_path")]
    public string ImageRelPath { get; set; }

    [JsonProperty("parent_msg_id")]
    public int ParentMessageId { get; set;}

    [JsonProperty("primary_msg_id")]
    public int PrimaryMessageId { get; set; }

    [JsonProperty("seen_msg_ids")]
    public IEnumerable<int> SeenMessageIds { get; set; } = new List<int>();
}