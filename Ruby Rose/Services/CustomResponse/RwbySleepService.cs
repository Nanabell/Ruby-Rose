using System;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using NLog;
using RubyRose.Common;
using RubyRose.Database;
using RubyRose.Database.Models;
using System.IO;

namespace RubyRose.Services.CustomResponse
{
    public class RwbySleepService
    {
        private readonly Logger _logger = LogManager.GetLogger("Custom Reactions");
        private readonly DiscordSocketClient _client;
        private readonly MongoClient _mongo;
        private readonly ConcurrentDictionary<ulong, bool> _ruby = new ConcurrentDictionary<ulong, bool>();
        private readonly ConcurrentDictionary<ulong, bool> _weiss = new ConcurrentDictionary<ulong, bool>();

        public RwbySleepService(IServiceProvider provider)
        {
            _client = provider.GetService<DiscordSocketClient>();
            _mongo = provider.GetService<MongoClient>();
        }

        internal Task StartService()
        {
            _client.MessageReceived += OnMessageReceived;
            return Task.CompletedTask;
        }

        private async Task OnMessageReceived(SocketMessage socketMessage)
        {
            if (socketMessage is SocketUserMessage message)
            {
                if (message.Author is SocketGuildUser)
                {
                    var context = new CommandContext(_client, message);
                    if (message.Channel.CheckChannelPermission(ChannelPermission.AttachFiles,
                        await context.Guild.GetCurrentUserAsync()))
                    {
                        if (Regex.IsMatch(context.Message.Content, "<:WSleeper:\\d+>", RegexOptions.Compiled))
                        {
                            if (_ruby.TryGetValue(context.Channel.Id, out var isRuby))
                            {
                                if (isRuby)
                                {
                                    await TryPostImage(context);
                                }
                                else
                                {
                                    _weiss.TryAdd(context.Channel.Id, true);
                                }
                            }
                            else
                            {
                                _weiss.TryAdd(context.Channel.Id, true);
                            }
                        }
                        else if (Regex.IsMatch(context.Message.Content, "<:RSleeper:\\d+>", RegexOptions.Compiled))
                        {
                            if (_weiss.TryGetValue(context.Channel.Id, out var isWeiss))
                            {
                                if (isWeiss)
                                {
                                    await TryPostImage(context);
                                }
                                else
                                {
                                    _ruby.TryAdd(context.Channel.Id, true);
                                }
                            }
                            else
                            {
                                _ruby.TryAdd(context.Channel.Id, true);
                            }
                        }
                        else
                        {
                            _ruby.TryRemove(context.Channel.Id, out var _);
                            _weiss.TryRemove(context.Channel.Id, out var _);
                        }
                    }
                }
            }
        }

        private async Task TryPostImage(ICommandContext context)
        {
            var settings = await _mongo.GetCollection<Settings>(_client).GetByGuildAsync(context.Guild.Id);
            if (settings.RwbySleeper)
            {
                var directory = Directory.GetCurrentDirectory();
                const string path = "/Data/RwbySleeper.png";
                _logger.Info("Triggered Rwby Sleeper png");
                await context.Channel.SendFileAsync(directory + path);
                _weiss.TryRemove(context.Channel.Id, out var _);
                _ruby.TryRemove(context.Channel.Id, out var _);
            }
        }
    }
}