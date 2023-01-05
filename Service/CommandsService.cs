﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using CharacterAI_Discord_Bot.Handlers;

namespace CharacterAI_Discord_Bot.Service
{
    public partial class CommandsService : CommonService
    {
        public static async Task AutoSetup(ServiceProvider services, DiscordSocketClient client)
        {
            if (Config is null) return;

            var integration = services.GetRequiredService<CommandHandler>().integration;
            integration.audienceMode = Config.DefaultAudienceMode;
            if (await integration.Setup(Config.AutoCharId, reset: false) is false) return;

            var charInfo = integration.charInfo;

            await UpdatePlayingStatus(integration.charInfo, client, integration.audienceMode).ConfigureAwait(false);
            await SetBotNickname(charInfo, client).ConfigureAwait(false);
            await SetBotAvatar(client.CurrentUser).ConfigureAwait(false);
        }

        public static async Task<Task> SetCharacterAsync(string charID, CommandHandler handler, SocketCommandContext context, bool reset = false)
        {
            var integration = handler.integration;

            if (handler.lastCharacterCallMsgId != 0)
            {
                var lastMessage = await context.Message.Channel.GetMessageAsync(handler.lastCharacterCallMsgId);
                await HandlerService.RemoveButtons(lastMessage).ConfigureAwait(false);
                handler.lastResponse.SetDefaults();
            }

            if (await integration.Setup(charID, reset: reset) is false)
                return Task.Run(() => context.Message.ReplyAsync("⚠️ Failed to set character!"));

            var charInfo = integration.charInfo;
            string reply = $"{charInfo.Greeting}\n*\"{charInfo.Description}\"*";

            try { await SetBotNickname(charInfo, context.Client).ConfigureAwait(false); }
            catch { reply += "\n⚠️ Failed to set bot name! Probably, missing permissions?"; }

            try { await SetBotAvatar(context.Client.CurrentUser).ConfigureAwait(false); }
            catch { reply += "\n⚠️ Failed to set bot avatar!"; }

            return Task.Run(() =>
            {
                UpdatePlayingStatus(charInfo, context.Client, integration.audienceMode).ConfigureAwait(false);
                context.Message.ReplyAsync(reply);
            });
        }

        public static async Task ResetCharacterAsync(CommandHandler handler, SocketCommandContext context)
        {
            string charId = handler.integration.charInfo.CharId!;
            await Task.Run(()
                => SetCharacterAsync(charId, handler, context, reset: true)).ConfigureAwait(false);
        }

        public static async Task SetBotNickname(Character charInfo, DiscordSocketClient client)
        {
            var guildID = client.Guilds.First().Id;
            var botAsGuildUser = client.GetGuild(guildID).GetUser(client.CurrentUser.Id);
            await botAsGuildUser.ModifyAsync(u => { u.Nickname = charInfo.Name; });
        }

        public static async Task SetBotAvatar(SocketSelfUser bot)
        {
            using var fs = new FileStream(avatarPath, FileMode.Open);
            await bot.ModifyAsync(u => { u.Avatar = new Discord.Image(fs); });
        }

        public static async Task UpdatePlayingStatus(Character charInfo, DiscordSocketClient client, bool amode)
        {
            string? charID = charInfo.CharId;
            string desc = charID is null ? "No character selected" : charInfo.Title!;

            await client.SetGameAsync($"Audience mode: " + (amode ? "✔️" : "✖️") + " | " + desc);
        }

        public static Task NoPermissionAlert(SocketCommandContext context)
        {
            if (string.IsNullOrEmpty(nopowerPath) || !File.Exists(nopowerPath))
                return Task.CompletedTask;
            
            var mRef = new MessageReference(context.Message.Id);
            Task.Run(() => context.Channel.SendFileAsync(nopowerPath, messageReference: mRef));

            return Task.CompletedTask;
        }

        public static bool ValidateBotRole(SocketCommandContext context)
        {
            var user = context.User as SocketGuildUser;
            if (user!.Id == context.Guild.OwnerId) return true;

            var roles = (user as IGuildUser).Guild.Roles;
            var requiredRole = roles.FirstOrDefault(role => role.Name == Config.BotRole);

            return user.Roles.Contains(requiredRole);
        }
    }
}
