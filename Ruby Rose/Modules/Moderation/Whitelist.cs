using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using MongoDB.Driver;
using RubyRose.Common.Preconditions;
using RubyRose.Database;
using NLog;
using RubyRose.Database.Models;

namespace RubyRose.Modules.Moderation
{
    [Name("Moderation"), Group]
    public class Whitelist : ModuleBase
    {
        [Group("Whitelist"), Name("Whitelist")]
        public class WhitelistCommands : ModuleBase
        {
            private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
            private readonly MongoClient _mongo;

            public WhitelistCommands(IDependencyMap map)
            {
                _mongo = map.Get<MongoClient>();
            }

            [Command("Add")]
            [Summary("Use this command to add a command to the whitelist for the current or given channel")]
            [MinPermission(AccessLevel.ServerModerator)]
            public async Task Add(CommandInfo command, ITextChannel channel = null)
            {
                channel = channel ?? Context.Channel as ITextChannel;
                if (channel != null)
                {
                    var newWhite = new Whitelists
                    {
                        GuildId = Context.Guild.Id,
                        ChannelId = channel.Id,
                        Name = command.Name
                    };

                    var allWhitelists = _mongo.GetCollection<Whitelists>(Context.Client);
                    var whitelists = await GetCommandWhitelists(allWhitelists, Context.Guild, command.Name);

                    if (whitelists != null)
                    {
                        if (!whitelists.Exists(c => c.ChannelId == channel.Id))
                        {
                            await allWhitelists.InsertOneAsync(newWhite);
                            await ReplyAsync($"Command `{command.Name}` is now Whitelisted to `{channel.Name}`");
                            Logger.Info($"Command {command.Name} now whitelisted to {channel.Name} on {Context.Guild.Name}");
                        }
                        else
                        {
                            await ReplyAsync($"Command {command.Name} already whitelisted to `{channel.Name}`");
                            Logger.Warn($"Failed to Whitelist {command.Name} to {channel.Name} on {Context.Guild.Name}, already Whitelisted");
                        }
                    }
                    else
                    {
                        await allWhitelists.InsertOneAsync(newWhite);
                        await ReplyAsync($"Command `{command.Name}` is now Whitelisted to `{channel.Name}`");
                        Logger.Info($"Command {command.Name} now whitelisted to {channel.Name} on {Context.Guild.Name}");
                    }
                }
            }

            [Command("Remove"), Alias("Rm")]
            [Summary("Use this command to remove a command from the whitelist for the current or a given channel")]
            [MinPermission(AccessLevel.ServerModerator)]
            public async Task Remove(CommandInfo command, ITextChannel channel = null)
            {
                channel = channel ?? Context.Channel as ITextChannel;
                var allWhitelists = _mongo.GetCollection<Whitelists>(Context.Client);
                var whitelists = await GetCommandWhitelists(allWhitelists, Context.Guild, command.Name);

                if (whitelists != null)
                {
                    if (whitelists.Exists(c => channel != null && c.ChannelId == channel.Id))
                    {
                        var whitelist = whitelists.First(c => channel != null && c.ChannelId == channel.Id);
                        await allWhitelists.DeleteAsync(whitelist);
                        await ReplyAsync($"Command `{command.Name}` is no longer Whitelisted to `{channel?.Name}`");
                        Logger.Info($"Command {command.Name} no longer whitelisted to {channel?.Name} on {Context.Guild.Name}");
                    }
                    else
                    {
                        await ReplyAsync($"Command {command.Name} not whitelisted to `{channel?.Name}`");
                        Logger.Warn($"Failed to remove Whitelist {command.Name} from {channel?.Name} on {Context.Guild.Name}, not Whitelisted");
                    }
                }
                else
                {
                    await ReplyAsync($"Command {command.Name} not whitelisted to `{channel?.Name}`");
                    Logger.Warn($"Failed to remove Whitelist {command.Name} from {channel?.Name} on {Context.Guild.Name}, not Whitelisted");
                }
            }

            [Command("All")]
            [Summary("Whitelist all Commands to the current or given Channel")]
            [MinPermission(AccessLevel.ServerModerator)]
            public async Task All(ITextChannel channel = null)
            {
                channel = channel ?? Context.Channel as ITextChannel;
                if (channel != null)
                {
                    var newWhite = new Whitelists
                    {
                        GuildId = Context.Guild.Id,
                        ChannelId = channel.Id,
                        Name = "all"
                    };

                    var allWhitelists = _mongo.GetCollection<Whitelists>(Context.Client);
                    var whitelists = await GetCommandWhitelists(allWhitelists, Context.Guild, "all");

                    if (whitelists != null)
                    {
                        if (!whitelists.Exists(c => c.ChannelId == channel.Id))
                        {
                            await allWhitelists.InsertOneAsync(newWhite);
                            await ReplyAsync($"All Commands are now Whitelisted to `{channel.Name}`");
                            Logger.Info($"All Commands now whitelisted to {channel.Name} on {Context.Guild.Name}");
                        }
                        else
                        {
                            await ReplyAsync($"All Commands are already whitelisted to `{channel.Name}`");
                            Logger.Warn($"Failed to Whitelist All Commands to {channel.Name} on {Context.Guild.Name}, already Whitelisted");
                        }
                    }
                    else
                    {
                        await allWhitelists.InsertOneAsync(newWhite);
                        await ReplyAsync($"All Commands are now Whitelisted to `{channel.Name}`");
                        Logger.Info($"All Commands are now whitelisted to {channel.Name} on {Context.Guild.Name}");
                    }
                }
            }

            [Command("None")]
            [Summary("Remove the current or given Channel from the Global-Whitelist")]
            [MinPermission(AccessLevel.ServerModerator)]
            public async Task None(ITextChannel channel = null)
            {
                channel = channel ?? Context.Channel as ITextChannel;
                var allWhitelists = _mongo.GetCollection<Whitelists>(Context.Client);
                var whitelists = await GetCommandWhitelists(allWhitelists, Context.Guild, "all");

                if (whitelists != null)
                {
                    if (whitelists.Exists(c => channel != null && c.ChannelId == channel.Id))
                    {
                        var whitelist = whitelists.First(c => channel != null && c.ChannelId == channel.Id);
                        await allWhitelists.DeleteAsync(whitelist);
                        await ReplyAsync($"All Commands are no longer Whitelisted to `{channel?.Name}`");
                        Logger.Info($"All Commands are no longer whitelisted to {channel?.Name} on {Context.Guild.Name}");
                    }
                    else
                    {
                        await ReplyAsync($"No Command is not whitelisted to `{channel?.Name}`");
                        Logger.Warn($"Failed to remove Whitelist for All Commands from {channel?.Name} on {Context.Guild.Name}, not Whitelisted");
                    }
                }
                else
                {
                    await ReplyAsync($"No Command is not whitelisted to `{channel?.Name}`");
                    Logger.Warn($"Failed to remove Whitelist for All Commands from {channel?.Name} on {Context.Guild.Name}, not Whitelisted");
                }
            }

            private static async Task<List<Whitelists>> GetCommandWhitelists(IMongoCollection<Whitelists> collection, IGuild guild, string name)
            {
                var whitelistsCursor = await collection.FindAsync(f => f.GuildId == guild.Id && f.Name == name);
                return await whitelistsCursor.ToListAsync();
            }
        }
    }
}