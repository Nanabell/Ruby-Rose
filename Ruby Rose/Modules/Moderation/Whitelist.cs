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
    public class Whitelist : ModuleBase
    {
        [Group("Whitelist"), Name("Whitelist")]
        public class WhitelistCommands : ModuleBase
        {
            private readonly CommandService _service;
            private readonly IDependencyMap _map;
            private readonly MongoClient _mongo;

            public WhitelistCommands(CommandService service, IDependencyMap map)
            {
                if (service == null) throw new ArgumentNullException(nameof(service));
                if (map == null) throw new ArgumentNullException(nameof(map));

                _service = service;
                _map = map;
                _mongo = map.Get<MongoClient>();
            }

            [Command("Add")]
            [Summary("Use this command to add a command to the whitelist for the current or given channel")]
            [MinPermission(AccessLevel.ServerModerator)]
            public async Task Add(string commandName, ITextChannel channel = null)
            {
                var result = _service.Search(Context, commandName);
                if (result.IsSuccess)
                {
                    var command = result.Commands.FirstOrDefault().Command;
                    if (channel == null) channel = Context.Channel as ITextChannel;
                    var wcollec = _mongo.GetDatabase(Context.Guild.Id.ToString()).GetCollection<CommandWhitelist>("CommandWhitelist");
                    var all = await wcollec.FindAsync("{}");
                    var cmdlist = await all.ToListAsync();

                    if (cmdlist.Any(n => n.Name == command.Name))
                    {
                        var cmd = cmdlist.First(n => n.Name == command.Name);
                        if (channel != null && !cmd.WhitelistedChannelIds.Contains(channel.Id))
                        {
                            cmd.WhitelistedChannelIds.Add(channel.Id);
                            var update = Builders<CommandWhitelist>.Update
                                .Set("WhitelistedChannelIds", cmd.WhitelistedChannelIds);

                            await wcollec.UpdateOneAsync(f => f.Name == command.Name, update);
                            await Context.Channel.SendEmbedAsync(Embeds.Success("Channel Whitelisted", $"Channel {channel.Mention} has been added to `{command.Name}'s` Whitelist"));
                        }
                        else await Context.Channel.SendEmbedAsync(Embeds.Invalid($"Channel {channel?.Name} already whitelisted for Command {command.Name}"));
                    }
                    else
                    {
                        if (channel != null)
                        {
                            var cw = new CommandWhitelist { Name = command.Name, WhitelistedChannelIds = new List<ulong>() { channel.Id } };

                            await wcollec.InsertOneAsync(cw);
                        }
                        await Context.Channel.SendEmbedAsync(Embeds.Success("Whitelist Created", $"The command `{command.Name}` is now whitelisted to the channel {channel?.Mention}"));
                    }
                }
                else await Context.Channel.SendEmbedAsync(Embeds.NotFound($"A Command with the name {commandName} was not found!"));
            }

            [Command("Remove"), Alias("Rm")]
            [Summary("Use this command to remove a command from the whitelist for the current or a given channel")]
            [MinPermission(AccessLevel.ServerModerator)]
            public async Task Remove(string commandName, ITextChannel channel = null)
            {
                var result = _service.Search(Context, commandName);
                if (result.IsSuccess)
                {
                    var command = result.Commands.FirstOrDefault().Command;
                    if (channel == null) channel = Context.Channel as ITextChannel;

                    var wcollec = _mongo.GetDatabase(Context.Guild.Id.ToString()).GetCollection<CommandWhitelist>("CommandWhitelist");
                    var all = await wcollec.FindAsync("{}");
                    var cmdlist = await all.ToListAsync();

                    if (cmdlist.Any(n => n.Name == command.Name))
                    {
                        var cmd = cmdlist.First(n => n.Name == command.Name);
                        if (channel != null && cmd.WhitelistedChannelIds.Contains(channel.Id))
                        {
                            if (cmd.WhitelistedChannelIds.Count > 1)
                            {
                                cmd.WhitelistedChannelIds.Remove(channel.Id);

                                var update = Builders<CommandWhitelist>.Update
                                    .Set("WhitelistedChannelIds", cmd.WhitelistedChannelIds);

                                await wcollec.UpdateOneAsync(f => f.Name == command.Name, update);
                                await Context.Channel.SendEmbedAsync(Embeds.Success("Channel Removed", $"Channel {channel.Mention} has been removed from `{command.Name}'s` whitelist"));
                            }
                            else
                            {
                                await wcollec.FindOneAndDeleteAsync(f => f.Name == command.Name);
                                await Context.Channel.SendEmbedAsync(Embeds.Success("Whitelist Removed", $"Command `{command.Name}` is no longer whitelisted"));
                            }
                        }
                        else await Context.Channel.SendEmbedAsync(Embeds.Invalid($"Channel {channel?.Mention} is not whitelisted for Command `{command.Name}`"));
                    }
                    else await Context.Channel.SendEmbedAsync(Embeds.Invalid($"Command {commandName} is not whitelisted and thus can't be removed"));
                }
                else await Context.Channel.SendEmbedAsync(Embeds.NotFound($"A Command with the name {commandName} was not found!"));
            }
            [Command("All")]
            [Summary("Whitelist all Commands to the current or given Channel")]
            [MinPermission(AccessLevel.ServerModerator)]
            public async Task All(ITextChannel channel = null)
            {
                if (channel == null) channel = Context.Channel as ITextChannel;
                var wcollec = _mongo.GetDatabase(Context.Guild.Id.ToString()).GetCollection<CommandWhitelist>("CommandWhitelist");
                var all = await wcollec.FindAsync("{}");
                var cmdlist = await all.ToListAsync();

                if (cmdlist.Any(n => n.Name == "all"))
                {
                    var cmd = cmdlist.First(n => n.Name == "all");
                    if (channel != null && !cmd.WhitelistedChannelIds.Contains(channel.Id))
                    {
                        cmd.WhitelistedChannelIds.Add(channel.Id);
                        var update = Builders<CommandWhitelist>.Update
                            .Set("WhitelistedChannelIds", cmd.WhitelistedChannelIds);

                        await wcollec.UpdateOneAsync(f => f.Name == "all", update);
                        await Context.Channel.SendEmbedAsync(Embeds.Success("Channel added to Global Override", $"Channel `{channel.Name}` has been added to the Global Override Whitelist"));
                    }
                    else await Context.Channel.SendEmbedAsync(Embeds.Invalid($"Global Override Whitelist already enabled for the Channel {channel?.Mention}"));
                }
                else
                {
                    if (channel != null)
                    {
                        var cw = new CommandWhitelist() { Name = "all", WhitelistedChannelIds = new List<ulong> { channel.Id } };

                        await wcollec.InsertOneAsync(cw);
                    }
                    await Context.Channel.SendEmbedAsync(Embeds.Success("Global Override created", $"Global Override Whitelist created and added Channel {channel?.Mention} to Override"));
                }
            }
            [Command("None")]
            [Summary("Remove the current or given Channel from the Global-Whitelist")]
            [MinPermission(AccessLevel.ServerModerator)]
            public async Task None(ITextChannel channel = null)
            {
                if (channel == null) channel = Context.Channel as ITextChannel;

                var wcollec = _mongo.GetDatabase(Context.Guild.Id.ToString()).GetCollection<CommandWhitelist>("CommandWhitelist");
                var all = await wcollec.FindAsync("{}");
                var cmdlist = await all.ToListAsync();

                if (cmdlist.Any(n => n.Name == "all"))
                {
                    var cmd = cmdlist.First(n => n.Name == "all");
                    if (channel != null && cmd.WhitelistedChannelIds.Contains(channel.Id))
                    {
                        if (cmd.WhitelistedChannelIds.Count > 1)
                        {
                            cmd.WhitelistedChannelIds.Remove(channel.Id);

                            var update = Builders<CommandWhitelist>.Update
                                .Set("WhitelistedChannelIds", cmd.WhitelistedChannelIds);

                            await wcollec.UpdateOneAsync(f => f.Name == "all", update);
                            await Context.Channel.SendEmbedAsync(Embeds.Success("Channel removed from Global Override", $"Channel {channel.Mention} has been removed from the Global Override Whitelist"));
                        }
                        else
                        {
                            await wcollec.FindOneAndDeleteAsync(f => f.Name == "all");
                            await Context.Channel.SendEmbedAsync(Embeds.Success("Override Deleted", $"Global Override deleted since it was empty"));
                        }
                    }
                    else await Context.Channel.SendEmbedAsync(Embeds.Invalid($"Global Override Whitelist for Channel {channel?.Mention} not enalbed and thus can't be disabled"));
                }
                else await Context.Channel.SendEmbedAsync(Embeds.NotFound($"Global Override Whitelist is Empty and thus nothing can be removed"));
            }
        }
    }
}