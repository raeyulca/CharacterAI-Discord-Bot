

namespace CharacterAI_Discord_Bot.Models;

public class ChatReply
{
    public ChatReply[] Replies { get; set;} = null;
    public int CurrentReply { get; set;} = 0;
    public int PrimaryMsgId { get; set;} = 0;
    public int LastUserMsgId { get; set;} = 0;
}