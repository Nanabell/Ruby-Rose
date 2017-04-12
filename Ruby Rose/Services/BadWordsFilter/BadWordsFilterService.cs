using Discord;
using Discord.Rest;
using Discord.WebSocket;
using MongoDB.Driver;
using RubyRose.Database;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RubyRose.Services
{
    public class BadWordsFilterService : ServiceBase
    {
        private static List<string> BadWords = new List<string>();

        protected override Task PreDisable()
        {
            Client.MessageReceived -= Client_MessageReceived;
            return Task.CompletedTask;
        }

        protected override async Task PreEnable()
        {
            await ReloadBadWords(Client, Map.Get<MongoClient>());
            Client.MessageReceived += Client_MessageReceived;
        }

        private async Task Client_MessageReceived(SocketMessage arg)
        {
            if (arg is SocketUserMessage message)
            {
                if (message.Author is SocketGuildUser user)
                {
                    var application = await Client.GetApplicationInfoAsync();

                    if (user.IsBot)
                        return;
                    if (!message.Channel.CheckChannelPermission(ChannelPermission.ManageMessages, user.Guild.CurrentUser))
                        return;
                    if (user.GuildPermissions.BanMembers)
                        return;
                    if (user.Id == application.Owner.Id)
                        return;

                    if (BadWords.Count > 0)
                    {
                        try
                        {
                            foreach (var filter in BadWords)
                            {
                                if (Regex.IsMatch(message.Content, filter))
                                {
                                    await message.DeleteAsync();
                                    await SendBadWordMessage(user, message.Channel, filter);
                                    _logger.Info($"Deleted Message with id {message.Id} due to Regex Match {filter}");
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            _logger.Warn(e, $"Failed to Delete Bad Word Reason:");
                        }
                    }
                }
            }
        }

        private async Task<RestUserMessage> SendBadWordMessage(SocketUser user, ISocketMessageChannel channel, string regex)
        {
            return await channel.SendMessageAsync($"{user.Mention}, This word / phrase is marked as BadWord with the Regex `{regex}`");
        }

        protected override bool WaitForReady()
            => true;

        public static async Task ReloadBadWords(DiscordSocketClient client, MongoClient mongo)
        {
            var allSettings = await mongo.GetCollection<Settings>(Client).Find("{}").ToListAsync();

            foreach (var settings in allSettings)
                BadWords = settings.BadWords;
        }
    }
}