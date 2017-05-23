using Discord.WebSocket;
using MongoDB.Driver;
using RubyRose.Database;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using RubyRose.Common;
using RubyRose.Database.Models;
using Microsoft.Extensions.DependencyInjection;

namespace RubyRose.Services.RwbyFight
{
    public class RwbyFightService : ServiceBase
    {
        private readonly Weiss _weissFirst = new Weiss();
        private readonly Ruby _rubyFirst = new Ruby();
        private static readonly ConcurrentDictionary<ulong, bool> IsRwbyFight = new ConcurrentDictionary<ulong, bool>();

        protected override Task PreDisable()
        {
            Client.MessageReceived -= Client_MessageReceived;
            return Task.CompletedTask;
        }

        protected override async Task PreEnable()
        {
            await ReloadRwbyFight(Client, Provider.GetService<MongoClient>());
            Client.MessageReceived += Client_MessageReceived;
        }

        protected override bool WaitForReady()
            => true;

        public static async Task ReloadRwbyFight(DiscordSocketClient client, MongoClient mongo)
        {
            var allSettings = await mongo.GetCollection<Settings>(Client).Find("{}").ToListAsync();

            foreach (var settings in allSettings)
                IsRwbyFight.AddOrUpdate(settings.GuildId, settings.RwbyFight, (key, oldvalue) => settings.RwbyFight);
        }

        private async Task Client_MessageReceived(SocketMessage arg)
        {
            if (arg is SocketUserMessage message)
            {
                if (message.Author is SocketGuildUser user)
                {
                    if (message.Channel.CheckChannelPermission(ChannelPermission.AttachFiles, user.Guild.CurrentUser))
                    {
                        if (IsRwbyFight.TryGetValue(user.Guild.Id, out var isEnabled))
                        {
                            if (isEnabled)
                            {
                                if (Regex.IsMatch(message.Content, "<:Heated2:\\d+>"))
                                {
                                    if (_weissFirst.TryGet(message.Channel.Id))
                                        await PostImage(message.Channel);
                                    else
                                        _rubyFirst.TryAdd(message.Channel.Id);
                                }
                                else if (Regex.IsMatch(arg.Content, "<:Heated1:\\d+>"))
                                {
                                    if (_rubyFirst.TryGet(message.Channel.Id))
                                        await PostImage(message.Channel);
                                    else
                                        _weissFirst.TryAdd(message.Channel.Id);
                                }
                                else
                                {
                                    _weissFirst.TryRemove(message.Channel.Id);
                                    _rubyFirst.TryRemove(message.Channel.Id);
                                }
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
            Logger.Info("Triggered Rwby Fight Gif");
            await channel.SendFileAsync($"{direc.FullName}/Data/rwby-fight.gif");
            _weissFirst.TryRemove(channel.Id);
            _rubyFirst.TryRemove(channel.Id);
        }
    }

    public class Ruby
    {
        internal List<ulong> RubyFirst = new List<ulong>();

        public bool TryGet(ulong channel)
         => RubyFirst.Contains(channel);

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
         => WeissFirst.Contains(channel);

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