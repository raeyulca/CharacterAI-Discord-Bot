﻿using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using Discord.WebSocket;
using Discord;
using CharacterAI_Discord_Bot.Models;

namespace CharacterAI_Discord_Bot.Service
{
    public partial class CommonService
    {
        public static readonly Configuration Config = GetConfig()!;
        public static readonly string imgPath = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + "img" + Path.DirectorySeparatorChar;
        public static readonly string avatarPath = imgPath + "characterAvatar.webp";
        public static readonly string defaultAvatarPath = imgPath + "defaultAvatar.webp";
        public static readonly string tempImgPath = imgPath + "temp.webp";
        public static readonly string nopowerPath = imgPath + Config.NoPower;

        public static string RemoveMention(string text)
        {
            text = text.Trim(' ');
            // Remove first mention
            if (text.StartsWith("<"))
                text = new Regex("\\<(.*?)\\>").Replace(text, "", 1);
            // Remove prefix
            foreach (string prefix in Config.BotPrefixes)
                text = text.Replace(prefix, "");

            return text;
        }

        public static async Task<byte[]?> DownloadImg(string url)
        {
            if (string.IsNullOrEmpty(url)) return null;

            HttpClient client = new();
            // Try 10 times and return null
            for (int i = 0; i < 10; i++)
            {
                try { return await client.GetByteArrayAsync(url); }
                catch { Thread.Sleep(2000); }
            }

            return null;
        }

        // Test feature that makes character aware that he's talking to many different people
        public static string MakeItThreadMessage(string text, SocketUserMessage message)
        {
            string author = message.Author.Username;
            if (!string.IsNullOrEmpty(text) && !string.IsNullOrWhiteSpace(text))
                text = $"User [{author}] says:\n{text}";
            if (message.ReferencedMessage != null)
                text = $"(In response to: \"{RemoveMention(message.ReferencedMessage.Content)}\")\n{text}";

            return text;
        }

        public static bool SuccessLog(string logText = "")
        {
            Log(logText + "\n", ConsoleColor.Green);

            return true;
        }

        public static bool FailureLog(string logText = "")
        {
            Log(logText + "\n", ConsoleColor.Red);

            return false;
        }

        public static void Log(string text, ConsoleColor color = ConsoleColor.Gray)
        {
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ResetColor();
        }

        public static Configuration GetConfig()
        {
            var path = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "Config.json";
            using StreamReader configJson = new StreamReader(path);
            var configParsed = JsonConvert.DeserializeObject<Configuration>(configJson.ReadToEnd())!;
            return configParsed;
        }

        // probably not useless
        //public static async Task CreateRole(DiscordSocketClient client)
        //{
        //    var guild = client.Guilds.FirstOrDefault();
        //    var role = client.GetGuild(guild.Id).Roles.FirstOrDefault(role => role.Name == CommonService.GetConfig().botRole);
        //    if (!string.IsNullOrEmpty(role.ToString)) return;

        //    try
        //    {
        //        Log("Creating role... ");
        //        var newRole = await guild.CreateRoleAsync(GetConfig().botRole).Result;
        //        await guild.Owner.AddRoleAsync(newRole);
        //    }
        //    catch { Failure("Failed to create default bot role. Probably, missing permissions?"); }

        //    Success("OK\n");
        //}
    }
}
