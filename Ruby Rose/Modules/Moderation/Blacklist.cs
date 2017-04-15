using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using MongoDB.Driver;
using RubyRose.Common.Preconditions;
using NLog;
using RubyRose.Database;
using RubyRose.Database.Models;

namespace RubyRose.Modules.Moderation
{
    [Name("Moderation"), Group]
    public class Blacklist : ModuleBase
    {
        [Group("Blacklist"), Name("Blacklist")]
        public class BlacklistCommands : ModuleBase
        {
            private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
            private readonly MongoClient _mongo;

            public BlacklistCommands(IDependencyMap map)
            {
                _mongo = map.Get<MongoClient>();
            }

            [Command("Add")]
            [Summary("Use this command to add a command to the blacklist for the current or given channel")]
            [MinPermission(AccessLevel.ServerModerator)]
            public async Task Add(CommandInfo command, ITextChannel channel = null)
            {
                channel = channel ?? Context.Channel as ITextChannel;
                if (channel != null)
                {
                    var newBlack = new Blacklists
                    {
                        GuildId = Context.Guild.Id,
                        ChannelId = channel.Id,
                        Name = command.Name
                    };

                    var allBlacklists = _mongo.GetCollection<Blacklists>(Context.Client);
                    var blacklists = await GetCommandBlacklists(allBlacklists, Context.Guild, command.Name);

                    if (blacklists != null)
                    {
                        if (!blacklists.Exists(c => c.ChannelId == channel.Id))
                        {
                            await allBlacklists.InsertOneAsync(newBlack);
                            await ReplyAsync($"Command `{command.Name}` is now Blacklisted in `{channel.Name}`");
                            Logger.Info($"Command {command.Name} now blacklisted in {channel.Name} on {Context.Guild.Name}");
                        }
                        else
                        {
                            await ReplyAsync($"Command {command.Name} already blacklisted in `{channel.Name}`");
                            Logger.Warn($"Failed to Blacklist {command.Name} in {channel.Name} on {Context.Guild.Name}, already Blacklisted");
                        }
                    }
                    else
                    {
                        await allBlacklists.InsertOneAsync(newBlack);
                        await ReplyAsync($"Command `{command.Name}` is now Blacklisted in `{channel.Name}`");
                        Logger.Info($"Command {command.Name} now blacklisted in {channel.Name} on {Context.Guild.Name}");
                    }
                }
            }

            [Command("Remove"), Alias("Rm")]
            [Summary("Use this command to remove a command from the blacklist for the current or a given channel")]
            [MinPermission(AccessLevel.ServerModerator)]
            public async Task Remove(CommandInfo command, ITextChannel channel = null)
            {
                channel = channel ?? Context.Channel as ITextChannel;
                var allBlacklists = _mongo.GetCollection<Blacklists>(Context.Client);
                var blacklists = await GetCommandBlacklists(allBlacklists, Context.Guild, command.Name);

                if (blacklists != null)
                {
                    if (blacklists.Exists(c => channel != null && c.ChannelId == channel.Id))
                    {
                        var whitelist = blacklists.First(c => channel != null && c.ChannelId == channel.Id);
                        await allBlacklists.DeleteAsync(whitelist);
                        await ReplyAsync($"Command `{command.Name}` is no longer Blacklisted in `{channel?.Name}`");
                        Logger.Info($"Command {command.Name} no longer blacklisted in {channel?.Name} on {Context.Guild.Name}");
                    }
                    else
                    {
                        await ReplyAsync($"Command {command.Name} not blacklisted in `{channel?.Name}`");
                        Logger.Warn($"Failed to remove Blacklist {command.Name} from {channel?.Name} on {Context.Guild.Name}, not Blacklsited");
                    }
                }
                else
                {
                    await ReplyAsync($"Command {command.Name} not Blacklisted in `{channel?.Name}`");
                    Logger.Warn($"Failed to remove Blacklist {command.Name} from {channel?.Name} on {Context.Guild.Name}, not Blacklisted");
                }
            }

            [Command("All")]
            [Summary("blacklist all Commands to the current or given Channel")]
            [MinPermission(AccessLevel.ServerModerator)]
            public async Task All(ITextChannel channel = null)
            {
                channel = channel ?? Context.Channel as ITextChannel;
                if (channel != null)
                {
                    var newBlack = new Blacklists
                    {
                        GuildId = Context.Guild.Id,
                        ChannelId = channel.Id,
                        Name = "all"
                    };

                    var allBlacklists = _mongo.GetCollection<Blacklists>(Context.Client);
                    var blacklists = await GetCommandBlacklists(allBlacklists, Context.Guild, "all");

                    if (blacklists != null)
                    {
                        if (!blacklists.Exists(c => c.ChannelId == channel.Id))
                        {
                            await allBlacklists.InsertOneAsync(newBlack);
                            await ReplyAsync($"All Commands are now Blacklisted in `{channel.Name}`");
                            Logger.Info($"All Commands now blacklisted in {channel.Name} on {Context.Guild.Name}");
                        }
                        else
                        {
                            await ReplyAsync($"All Commands are already Blacklisted in `{channel.Name}`");
                            Logger.Warn($"Failed to blacklist All Commands to {channel.Name} on {Context.Guild.Name}, already Blacklisted");
                        }
                    }
                    else
                    {
                        await allBlacklists.InsertOneAsync(newBlack);
                        await ReplyAsync($"All Commands are now Blacklisted to `{channel.Name}`");
                        Logger.Info($"All Commands are now blacklisted to {channel.Name} on {Context.Guild.Name}");
                    }
                }
            }

            [Command("None")]
            [Summary("Remove the current or given Channel from the Global-Blacklist")]
            [MinPermission(AccessLevel.ServerModerator)]
            public async Task None(ITextChannel channel = null)
            {
                channel = channel ?? Context.Channel as ITextChannel;
                var allBlacklists = _mongo.GetCollection<Blacklists>(Context.Client);
                var blacklists = await GetCommandBlacklists(allBlacklists, Context.Guild, "all");

                if (blacklists != null)
                {
                    if (blacklists.Exists(c => channel != null && c.ChannelId == channel.Id))
                    {
                        var blacklist = blacklists.First(c => channel != null && c.ChannelId == channel.Id);
                        await allBlacklists.DeleteAsync(blacklist);
                        await ReplyAsync($"All Commands are no longer Blacklisted in `{channel?.Name}`");
                        Logger.Info($"All Commands are no longer blacklsited in {channel?.Name} on {Context.Guild.Name}");
                    }
                    else
                    {
                        await ReplyAsync($"No Command is not Blacklisted in `{channel?.Name}`");
                        Logger.Warn($"Failed to remove Blacklist for All Commands from {channel?.Name} on {Context.Guild.Name}, not Blacklisted");
                    }
                }
                else
                {
                    await ReplyAsync($"No Command is not Blacklisted in `{channel?.Name}`");
                    Logger.Warn($"Failed to remove blacklist for All Commands from {channel?.Name} on {Context.Guild.Name}, not Blacklisted");
                }
            }

            private static async Task<List<Blacklists>> GetCommandBlacklists(IMongoCollection<Blacklists> collection, IGuild guild, string name)
            {
                var blacklistsCursor = await collection.FindAsync(f => f.GuildId == guild.Id && f.Name == name);
                return await blacklistsCursor.ToListAsync();
            }
        }
    }
}