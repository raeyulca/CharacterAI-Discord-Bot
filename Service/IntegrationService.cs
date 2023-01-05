using System.Dynamic;

namespace CharacterAI_Discord_Bot.Service
{
    public partial class IntegrationService : CommonService
    {
        public static void SetupCompleteLog(Character charInfo)
        {
            Log("\nCharacterAI - Connected\n\n", ConsoleColor.Green);
            Log($" [{charInfo.Name}]\n\n", ConsoleColor.Cyan);
            Log($"{charInfo.Greeting}\n");
            if (!string.IsNullOrEmpty(charInfo.Description))
                Log($"\"{charInfo.Description}\"\n");
            Log("\nSetup complete\n", ConsoleColor.Yellow);
        }
    }
}
