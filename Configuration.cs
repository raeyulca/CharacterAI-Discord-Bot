using Newtonsoft.Json;
using System.Collections.Generic;

namespace CharacterAI_Discord_Bot;

public class Config
{
    [JsonProperty("char_ai_user_token")]
    public string UserToken { get; set; }

    [JsonProperty("discord_bot_token")]
    public string BotToken { get; set; }

    [JsonProperty("discord_bot_role")]
    public string BotRole { get; set; }

    [JsonProperty("discord_bot_prefixes")]
    public IEnumerable<string> BotPrefixes { get; set; } = new List<string>();

    [JsonProperty("default_audience_mode")]
    public bool DefaultAudienceMode { get; set; }

    [JsonProperty("DefaultNoPermissionFile")]
    public string NoPower { get; set; }

    [JsonProperty("rate_limit")]
    public int RateLimit { get; set; }

    [JsonProperty("auto_buttons_remove")]
    public bool AutoRemove { get; set; }

    [JsonProperty("auto_setup")]
    public bool AutoSetupEnabled { get; set; }

    [JsonProperty("auto_char_id")]
    public string AutoCharId { get; set; }
}