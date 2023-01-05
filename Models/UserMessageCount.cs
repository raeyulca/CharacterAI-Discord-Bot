namespace CharacterAI_Discord_Bot.Models;

public class UserMessageCount
{
    public ulong UserId { get; set;}
    public int Minute { get; set; }
    public int Count { get; set; }
}