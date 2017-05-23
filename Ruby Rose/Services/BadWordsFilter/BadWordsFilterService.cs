using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using MongoDB.Driver;
using RubyRose.Common;
using RubyRose.Database;
using RubyRose.Database.Models;
using Microsoft.Extensions.DependencyInjection;

namespace RubyRose.Services.BadWordsFilter
{
    public class BadWordsFilterService : ServiceBase
    {
        private static List<string> _badWords = new List<string>();

        protected override Task PreDisable()
        {
            Client.MessageReceived -= Client_MessageReceived;
            return Task.CompletedTask;
        }

        protected override async Task PreEnable()
        {
            await ReloadBadWords(Client, Provider.GetService<MongoClient>());
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
                    if (user.Guild.OwnerId == user.Id)
                        return;
                    if (user.Id == application.Owner.Id)
                        return;

                    if (_badWords.Count > 0)
                    {
                        try
                        {
                            foreach (var filter in _badWords)
                            {
                                if (!Regex.IsMatch(message.Content, filter, RegexOptions.None,
                                    TimeSpan.FromSeconds(2))) continue;
                                await message.DeleteAsync();
                                await SendBadWordMessage(user, message.Channel);
                                Logger.Info($"Deleted Message with id {message.Id}");
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.Warn(e, "Failed to Delete Bad Word Reason:");
                        }
                    }
                }
            }
        }

        private static async Task<RestUserMessage> SendBadWordMessage(IMentionable user, ISocketMessageChannel channel)
        {
            return await channel.SendMessageAsync($"{user.Mention}, This Word / Phrase is marked as a bad Word!");
        }

        protected override bool WaitForReady()
            => true;

        public static async Task ReloadBadWords(DiscordSocketClient client, MongoClient mongo)
        {
            var allSettings = await mongo.GetCollection<Settings>(Client).Find("{}").ToListAsync();

            foreach (var settings in allSettings)
                _badWords = settings.BadWords;
        }
    }
}