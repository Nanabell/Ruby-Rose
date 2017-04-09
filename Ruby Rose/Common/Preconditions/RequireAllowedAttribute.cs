using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using MongoDB.Driver;
using System.Collections.Generic;
using RubyRose.Database;

namespace RubyRose
{
    public class RequireAllowedAttribute : PreconditionAttribute
    {
        private static MongoClient _mongo;

        public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command,
            IDependencyMap map)
        {
            _mongo = map.Get<MongoClient>();
            var user = context.User as IGuildUser;
            if (user == null) return Task.FromResult(PreconditionResult.FromSuccess());

            var application = context.Client.GetApplicationInfoAsync().GetAwaiter().GetResult();
            if (application.Owner.Id == context.User.Id) return Task.FromResult(PreconditionResult.FromSuccess());

            if (!IsWhitelisted(context, command))
                return IsBlacklisted(context, command)
                    ? Task.FromResult(
                        PreconditionResult.FromError(
                            $"Command __`{command.Name}`__ is **Blacklisted** to specific channels which include this channel, thus this command cant be used here!"))
                    : Task.FromResult(PreconditionResult.FromSuccess());
            return IsAllowed(context, command)
                ? Task.FromResult(PreconditionResult.FromSuccess())
                : Task.FromResult(
                    PreconditionResult.FromError(
                        $"Command __`{command.Name}`__ is **Whitelisted** to specific channels which do not incluse this channel, thus this command cant be used here!"));
        }

        private bool IsWhitelisted(ICommandContext context, CommandInfo info)
        {
            var allWhitelists = _mongo.GetCollection<Whitelists>(context.Client);
            var whitelistsAll = GetCommandWhitelists(allWhitelists, context.Guild, "all").GetAwaiter().GetResult();
            var commandWhitelists = GetCommandWhitelists(allWhitelists, context.Guild, info.Name).GetAwaiter().GetResult();

            return commandWhitelists.Any() || whitelistsAll.Any();
        }

        private bool IsBlacklisted(ICommandContext context, CommandInfo info)
        {
            var allBlacklists = _mongo.GetCollection<Blacklists>(context.Client);
            var blacklistsAll = GetCommandBlacklists(allBlacklists, context.Guild, "all").GetAwaiter().GetResult();
            var commandBlacklists = GetCommandBlacklists(allBlacklists, context.Guild, info.Name).GetAwaiter().GetResult();

            return blacklistsAll.Exists(b => b.ChannelId == context.Channel.Id) || commandBlacklists.Exists(b => b.ChannelId == context.Channel.Id);
        }

        private bool IsAllowed(ICommandContext context, CommandInfo info)
        {
            var allWhitelists = _mongo.GetCollection<Whitelists>(context.Client);
            var whitelistsAll = GetCommandWhitelists(allWhitelists, context.Guild, "all").GetAwaiter().GetResult();
            var commandWhitelists = GetCommandWhitelists(allWhitelists, context.Guild, info.Name).GetAwaiter().GetResult();

            return commandWhitelists.Exists(w => w.ChannelId == context.Channel.Id) || whitelistsAll.Exists(w => w.ChannelId == context.Channel.Id);
        }

        private async Task<List<Whitelists>> GetCommandWhitelists(IMongoCollection<Whitelists> collection, IGuild guild, string name)
        {
            var whitelistsCursor = await collection.FindAsync(f => f.GuildId == guild.Id && f.Name == name);
            return await whitelistsCursor.ToListAsync();
        }

        private async Task<List<Blacklists>> GetCommandBlacklists(IMongoCollection<Blacklists> collection, IGuild guild, string name)
        {
            var blacklistsCursor = await collection.FindAsync(f => f.GuildId == guild.Id && f.Name == name);
            return await blacklistsCursor.ToListAsync();
        }
    }
}