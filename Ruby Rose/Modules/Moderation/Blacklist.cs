using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using MongoDB.Driver;
using RubyRose.Common;
using RubyRose.Common.Preconditions;
using RubyRose.MongoDB;

namespace RubyRose.Modules.Moderation
{
    [Name("Moderation"), Group]
    public class Blacklist : ModuleBase
    {
        [Group("Blacklist"), Name("Blacklist")]
        public class BlacklistCommands : ModuleBase
        {
            private readonly CommandService _service;
            private readonly IDependencyMap _map;
            private readonly MongoClient _mongo;

            public BlacklistCommands(CommandService service, IDependencyMap map)
            {
                if (service == null) throw new ArgumentNullException(nameof(service));
                if (map == null) throw new ArgumentNullException(nameof(map));

                _service = service;
                _map = map;
                _mongo = map.Get<MongoClient>();
            }

            [Command("Add")]
            [Summary("Use this command to add a command to the blacklist for the current or given channel")]
            [MinPermission(AccessLevel.ServerModerator)]
            public async Task Add(string commandName, ITextChannel channel = null)
            {
                var result = _service.Search(Context, commandName);
                if (result.IsSuccess)
                {
                    var command = result.Commands.FirstOrDefault().Command;
                    if (channel == null) channel = Context.Channel as ITextChannel;
                    var bcollec = _mongo.GetDatabase(Context.Guild.Id.ToString()).GetCollection<CommandBlacklist>("CommandBlacklist");
                    var all = await bcollec.FindAsync("{}");
                    var cmdlist = await all.ToListAsync();

                    if (cmdlist.Any(n => n.Name == command.Name))
                    {
                        var cmd = cmdlist.First(n => n.Name == command.Name);
                        if (channel != null && !cmd.BlacklistedChannelIds.Contains(channel.Id))
                        {
                            cmd.BlacklistedChannelIds.Add(channel.Id);
                            var update = Builders<CommandBlacklist>.Update
                                .Set("BlacklistedChannelIds", cmd.BlacklistedChannelIds);

                            await bcollec.UpdateOneAsync(f => f.Name == command.Name, update);
                            await Context.Channel.SendEmbedAsync(Embeds.Success("Channel Added to Blacklist", $"Channel {channel.Mention} has been added to `{command.Name}'s` Blacklist"));
                        }
                        else await Context.Channel.SendEmbedAsync(Embeds.Invalid($"Channel {channel?.Name} already blacklisted for Command {command.Name}"));
                    }
                    else
                    {
                        if (channel != null)
                        {
                            var cw = new CommandBlacklist() { Name = command.Name, BlacklistedChannelIds = new List<ulong> { channel.Id } };

                            await bcollec.InsertOneAsync(cw);
                        }
                        await Context.Channel.SendEmbedAsync(Embeds.Success("Blacklist Created", $"The command `{command.Name}` is now blacklisted in the channel {channel?.Mention}"));
                    }
                }
                else await Context.Channel.SendEmbedAsync(Embeds.NotFound($"A Command with the name {commandName} was not found!"));
            }

            [Command("Remove"), Alias("Rm")]
            [Summary("Use this command to remove a command from the blacklist for the current or a given channel")]
            [MinPermission(AccessLevel.ServerModerator)]
            public async Task Remove(string commandName, ITextChannel channel = null)
            {
                var result = _service.Search(Context, commandName);
                if (result.IsSuccess)
                {
                    var command = result.Commands.FirstOrDefault().Command;
                    if (channel == null) channel = Context.Channel as ITextChannel;
                    var bcollec = _mongo.GetDatabase(Context.Guild.Id.ToString()).GetCollection<CommandBlacklist>("CommandBlacklist");
                    var all = await bcollec.FindAsync("{}");
                    var cmdlist = await all.ToListAsync();

                    if (cmdlist.Any(n => n.Name == command.Name))
                    {
                        var cmd = cmdlist.First(n => n.Name == command.Name);
                        if (channel != null && cmd.BlacklistedChannelIds.Contains(channel.Id))
                        {
                            if (cmd.BlacklistedChannelIds.Count > 1)
                            {
                                cmd.BlacklistedChannelIds.Remove(channel.Id);

                                var update = Builders<CommandBlacklist>.Update
                                    .Set("BlacklistedChannelIds", cmd.BlacklistedChannelIds);

                                await bcollec.UpdateOneAsync(f => f.Name == command.Name, update);
                                await Context.Channel.SendEmbedAsync(Embeds.Success("Channel Removed from Blacklist", $"Channel {channel.Mention} has been removed from `{command.Name}'s` blacklist"));
                            }
                            else
                            {
                                await bcollec.FindOneAndDeleteAsync(f => f.Name == command.Name);
                                await Context.Channel.SendEmbedAsync(Embeds.Success("Blacklist Deleted", $"The Blacklist for the Command {command.Name} was deleted since it was empty"));
                            }
                        }
                        else await Context.Channel.SendEmbedAsync(Embeds.Invalid($"Channel {channel?.Mention} is not blacklisted for Command `{command.Name}`"));
                    }
                    else await Context.Channel.SendEmbedAsync(Embeds.NotFound($"Command {commandName} is not blacklisted"));
                }
                else await Context.Channel.SendEmbedAsync(Embeds.NotFound($"A Command with the name {commandName} was not found!"));
            }
            [Command("All")]
            [Summary("blacklist all Commands to the current or given Channel")]
            [MinPermission(AccessLevel.ServerModerator)]
            public async Task All(ITextChannel channel = null)
            {
                if (channel == null) channel = Context.Channel as ITextChannel;
                var bcollec = _mongo.GetDatabase(Context.Guild.Id.ToString()).GetCollection<CommandBlacklist>("CommandBlacklist");
                var all = await bcollec.FindAsync("{}");
                var cmdlist = await all.ToListAsync();

                if (cmdlist.Any(n => n.Name == "all"))
                {
                    var cmd = cmdlist.First(n => n.Name == "all");
                    if (channel != null && !cmd.BlacklistedChannelIds.Contains(channel.Id))
                    {
                        cmd.BlacklistedChannelIds.Add(channel.Id);
                        var update = Builders<CommandBlacklist>.Update
                            .Set("BlacklistedChannelIds", cmd.BlacklistedChannelIds);

                        await bcollec.UpdateOneAsync(f => f.Name == "all", update);
                        await Context.Channel.SendEmbedAsync(Embeds.Success("Channel Blacklisted", $"All Commands have been blacklisted from {channel.Mention}"));
                    }
                    else await Context.Channel.SendEmbedAsync(Embeds.Invalid($"All Commands are already blacklisted from {channel?.Mention}"));
                }
                else
                {
                    if (channel != null)
                    {
                        var cw = new CommandBlacklist() { Name = "all", BlacklistedChannelIds = new List<ulong> { channel.Id } };

                        await bcollec.InsertOneAsync(cw);
                    }
                    await Context.Channel.SendEmbedAsync(Embeds.Success("Channel Blacklisted", $"All Commands have been blacklisted from {channel?.Mention}"));
                }
            }
            [Command("None")]
            [Summary("Remove the current or given Channel from the Global-Blacklist")]
            [MinPermission(AccessLevel.ServerModerator)]
            public async Task None(ITextChannel channel = null)
            {
                if (channel == null) channel = Context.Channel as ITextChannel;

                var wcollec = _mongo.GetDatabase(Context.Guild.Id.ToString()).GetCollection<CommandBlacklist>("CommandBlacklist");
                var all = await wcollec.FindAsync("{}");
                var cmdlist = await all.ToListAsync();

                if (cmdlist.Any(n => n.Name == "all"))
                {
                    var cmd = cmdlist.First(n => n.Name == "all");
                    if (channel != null && cmd.BlacklistedChannelIds.Contains(channel.Id))
                    {
                        if (cmd.BlacklistedChannelIds.Count > 1)
                        {
                            cmd.BlacklistedChannelIds.Remove(channel.Id);

                            var update = Builders<CommandBlacklist>.Update
                                .Set("BlacklistedChannelIds", cmd.BlacklistedChannelIds);

                            await wcollec.UpdateOneAsync(f => f.Name == "all", update);
                            await Context.Channel.SendEmbedAsync(Embeds.Success("Channel Override Removed", $"Global Override for {channel.Mention} Removed\nBeware that Command specific Blacklists will still block those Commands"));
                        }
                        else
                        {
                            await wcollec.FindOneAndDeleteAsync(f => f.Name == "all");
                            await Context.Channel.SendEmbedAsync(Embeds.Success("Global Override Removed", $"Global Override Removed since it was Empty\nBeware that Command specific Blacklists will still block those Commands"));
                        }
                    }
                    else await Context.Channel.SendEmbedAsync(Embeds.NotFound($"{channel?.Mention} is not in the Global Override Blacklist and thus can't be removed"));
                }
                else await Context.Channel.SendEmbedAsync(Embeds.NotFound("Global Override Blacklist is empty and thus nothing can be removed"));
            }
        }
    }
}