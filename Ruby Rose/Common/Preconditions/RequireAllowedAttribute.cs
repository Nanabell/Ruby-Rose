using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using MongoDB.Driver;
using RubyRose.MongoDB;

namespace RubyRose.Common.Preconditions
{
    public class RequireAllowedAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command,
            IDependencyMap map)
        {
            var user = context.User as IGuildUser;
            if (user == null) return Task.FromResult(PreconditionResult.FromSuccess());

            var application = context.Client.GetApplicationInfoAsync().GetAwaiter().GetResult();
            if (application.Owner.Id == context.User.Id) return Task.FromResult(PreconditionResult.FromSuccess());

            if (!IsWhitelisted(context, command, map))
                return IsBlacklisted(context, command, map)
                    ? Task.FromResult(
                        PreconditionResult.FromError(
                            $"Command __`{command.Name}`__ is **blacklisted** to specific channels which include this channel, thus this command cant be used here!"))
                    : Task.FromResult(PreconditionResult.FromSuccess());
            return IsAllowed(context, command, map)
                ? Task.FromResult(PreconditionResult.FromSuccess())
                : Task.FromResult(
                    PreconditionResult.FromError(
                        $"Command __`{command.Name}`__ is **whitelisted** to specific channels which do not incluse this channel, thus this command cant be used here!"));
        }

        private static bool IsWhitelisted(ICommandContext context, CommandInfo info, IDependencyMap map)
        {
            var mongo = map.Get<MongoClient>();

            var wcollec = mongo.GetDatabase($"{context.Guild.Id}").GetCollection<CommandWhitelist>("CommandWhitelist");
            var commandWhitelist = wcollec.Find("{}").ToList();

            return commandWhitelist.Any(command => command.Name == "all") || commandWhitelist.Any(command => command.Name == info.Name);
        }

        private static bool IsBlacklisted(ICommandContext context, CommandInfo info, IDependencyMap map)
        {
            var mongo = map.Get<MongoClient>();

            var bcollec = mongo.GetDatabase($"{context.Guild.Id}").GetCollection<CommandBlacklist>("CommandBlacklist");
            var commandBlacklist = bcollec.Find("{}").ToList();

            if (commandBlacklist.Any(command => command.Name == "all"))
            {
                var cmd = commandBlacklist.First(command => command.Name == "all");
                if (cmd.BlacklistedChannelIds.Contains(context.Channel.Id)) return true;
            }

            // ReSharper disable once InvertIf
            if (commandBlacklist.Any(command => command.Name == info.Name))
            {
                var cmd = commandBlacklist.First(command => command.Name == info.Name);
                if (cmd.BlacklistedChannelIds.Contains(context.Channel.Id)) return true;
            }
            return false;
        }

        private static bool IsAllowed(ICommandContext context, CommandInfo info, IDependencyMap map)
        {
            var mongo = map.Get<MongoClient>();

            var wcollec = mongo.GetDatabase($"{context.Guild.Id}").GetCollection<CommandWhitelist>("CommandWhitelist");
            var commandWhitelist = wcollec.Find("{}").ToList();

            if (commandWhitelist.Any(command => command.Name == info.Name))
            {
                var cmd = commandWhitelist.First(command => command.Name == info.Name);
                if (cmd.WhitelistedChannelIds.Contains(context.Channel.Id)) return true;
            }

            // ReSharper disable once InvertIf
            if (commandWhitelist.Any(command => command.Name == "all"))
            {
                var cmd = commandWhitelist.First(command => command.Name == "all");
                if (cmd.WhitelistedChannelIds.Contains(context.Channel.Id)) return true;
            }
            return false;
        }
    }
}