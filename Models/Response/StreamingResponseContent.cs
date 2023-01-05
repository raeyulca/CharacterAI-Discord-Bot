using Newtonsoft.Json;

namespace CharacterAI_Discord_Bot.Models.Response;

public class StreamingResponseContent
{
    [JsonProperty("is_final_chunk")]
    public bool IsFinalChunk { get; set; }
    
    [JsonProperty("last_user_msg_id")]
    public int LastUserMsgId { get; set; }
    
    [JsonProperty("replies")]
    public List<Reply> Replies { get; set; }

    [JsonProperty("src_char")]
    public ChatChar SrcChar { get; set; }

}

public class Reply
{
    [JsonProperty("text")]
    public string Text { get; set; }

    [JsonProperty("id")]
    public ulong Id { get; set; }
}

public class ChatChar
{
    [JsonProperty("avatar_file_name")]
    public string AvatarFileName { get; set; }

    [JsonProperty("participant")]
    public string Participant { get; set; }
}

public class Participant 
{ 
    [JsonProperty("name")]
    public string Name { get; set; }
}