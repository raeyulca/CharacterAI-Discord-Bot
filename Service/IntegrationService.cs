using CharacterAI_Discord_Bot.Models.Request;

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
        
        public static StreamingRequestContent BasicCallContent(Character charInfo, string msg, string imgPath)
        {
            var content = new StreamingRequestContent()
            {
                CharacterExternalId = charInfo.CharId!,
                EnableTTI = true,
                HistoryExternalId = charInfo.HistoryExternalId!,
                Text = msg,
                Tgt = charInfo.Tgt!,
                RankingMethod = "random",
                Staging = false,
                StreamEveryNSteps = 16,
                ChunksToPad = 8,
                IsProactive = false,
            };

            if (!string.IsNullOrEmpty(imgPath))
            {
                content.ImageDescriptionType = "AUTO_IMAGE_CAPTIONING";
                content.ImageOriginType = "UPLOADED";
                content.ImageRelPath = imgPath;
            }

            

            return content;
        }
    }
}
