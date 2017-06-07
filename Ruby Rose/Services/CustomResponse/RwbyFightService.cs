﻿using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
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

namespace RubyRose.Services.CustomResponse
{
    public class RwbyFightService
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly DiscordSocketClient _client;
        private readonly MongoClient _mongo;
        private readonly ConcurrentDictionary<ulong, bool> _ruby = new ConcurrentDictionary<ulong, bool>();
        private readonly ConcurrentDictionary<ulong, bool> _weiss = new ConcurrentDictionary<ulong, bool>();

        public RwbyFightService(IServiceProvider provider)
        {
            _client = provider.GetService<DiscordSocketClient>();
            _mongo = provider.GetService<MongoClient>();
        }

        internal Task StartService()
        {
            _client.MessageReceived += Client_MessageReceived;

            return Task.CompletedTask;
        }

        private async Task Client_MessageReceived(SocketMessage arg)
        {
            if (arg is SocketUserMessage message)
            {
                if (message.Author is SocketGuildUser user)
                {
                    if (message.Channel.CheckChannelPermission(ChannelPermission.AttachFiles, user.Guild.CurrentUser))
                    {
                        var context = new CommandContext(_client, message);
                        if (Regex.IsMatch(context.Message.Content, "<:Heated2:\\d+>", RegexOptions.Compiled))
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
                        else if (Regex.IsMatch(context.Message.Content, "<:Heated1:\\d+>", RegexOptions.Compiled))
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
                        else
                        {
                            _weiss.TryRemove(context.Channel.Id, out var _);
                            _ruby.TryRemove(context.Channel.Id, out var _);
                        }
                    }
                }
            }
        }

        private async Task TryPostImage(ICommandContext context)
        {
            var settings = await _mongo.GetCollection<Settings>(_client).GetByGuildAsync(context.Guild.Id);
            if (settings.RwbyFight)
            {
                var directory = Directory.GetCurrentDirectory();
                const string path = "/Data/rwby-fight.gif";
                _logger.Info("Triggered Rwby Fight Gif");
                await context.Channel.SendFileAsync(directory + path);
                _weiss.TryRemove(context.Channel.Id, out var _);
                _ruby.TryRemove(context.Channel.Id, out var _);
            }
        }
    }
}