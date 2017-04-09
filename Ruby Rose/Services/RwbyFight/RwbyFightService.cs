using Discord.WebSocket;
using MongoDB.Driver;
using NLog;
using RubyRose.Database;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System;

namespace RubyRose.Services.RwbyFight
{
    public class RwbyFightService : ServiceBase
    {
        private Weiss WeissFirst = new Weiss();
        private Ruby RubyFirst = new Ruby();
        private ConcurrentDictionary<ulong, bool> IsRwbyFight = new ConcurrentDictionary<ulong, bool>();

        protected override Task PreDisable()
        {
            Client.MessageReceived -= Client_MessageReceived;
            return Task.CompletedTask;
        }

        protected override async Task PreEnable()
        {
            await ReloadRwbyFight(Client, Map.Get<MongoClient>());
            Client.MessageReceived += Client_MessageReceived;
        }

        protected override bool WaitForReady()
            => true;

        public async Task ReloadRwbyFight(DiscordSocketClient client, MongoClient mongo)
        {
            var allSettings = await mongo.GetCollection<Settings>(Client).Find("{}").ToListAsync();

            foreach (var settings in allSettings)
                IsRwbyFight.TryAdd(settings.GuildId, settings.RwbyFight);
        }

        private async Task Client_MessageReceived(SocketMessage arg)
        {
            if (arg is SocketUserMessage message)
            {
                if (message.Author is SocketGuildUser user)
                {
                    if (IsRwbyFight.TryGetValue(user.Guild.Id, out var IsEnabled))
                    {
                        if (IsEnabled)
                        {
                            if (Regex.IsMatch(message.Content, "<:Heated2:\\d+>"))
                            {
                                if (WeissFirst.TryGet(message.Channel.Id))
                                    await PostImage(message.Channel);
                                else
                                    RubyFirst.TryAdd(message.Channel.Id);
                            }
                            else if (Regex.IsMatch(arg.Content, "<:Heated1:\\d+>"))
                            {
                                if (RubyFirst.TryGet(message.Channel.Id))
                                    await PostImage(message.Channel);
                                else
                                    WeissFirst.TryAdd(message.Channel.Id);
                            }
                        }
                    }
                }
            }
        }

        private async Task PostImage(ISocketMessageChannel channel)
        {
            var direc = new DirectoryInfo(Assembly.GetEntryAssembly().Location);
            do
            {
                direc = direc.Parent;
            }
            while (direc.Name != "Ruby Rose");
            _logger.Info("Triggered Rwby Fight Gif");
            await channel.SendFileAsync($"{direc.FullName}/Data/rwby-fight.gif");
            WeissFirst.TryRemove(channel.Id);
            RubyFirst.TryRemove(channel.Id);
        }
    }

    public class Ruby
    {
        internal List<ulong> RubyFirst = new List<ulong>();

        public bool TryGet(ulong channel)
         => RubyFirst.Contains(channel) ? true : false;

        public bool TryAdd(ulong channel)
        {
            if (!RubyFirst.Contains(channel))
            {
                RubyFirst.Add(channel);
                return true;
            }
            else return false;
        }

        public bool TryRemove(ulong channel)
        {
            if (RubyFirst.Contains(channel))
            {
                RubyFirst.Remove(channel);
                return true;
            }
            else return false;
        }
    }

    public class Weiss
    {
        internal List<ulong> WeissFirst = new List<ulong>();

        public bool TryGet(ulong channel)
         => WeissFirst.Contains(channel) ? true : false;

        public bool TryAdd(ulong channel)
        {
            if (!WeissFirst.Contains(channel))
            {
                WeissFirst.Add(channel);
                return true;
            }
            else return false;
        }

        public bool TryRemove(ulong channel)
        {
            if (WeissFirst.Contains(channel))
            {
                WeissFirst.Remove(channel);
                return true;
            }
            else return false;
        }
    }
}