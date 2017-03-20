using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace RubyRose.Common.Preconditions
{
    public enum AccessLevel
    {
        Blocked,
        IsPrivate,
        User,
        Trusted,
        ServerModerator,
        ServerOwner,
        BotOwner
    }
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class MinPermissionAttribute : PreconditionAttribute
    {
        private readonly AccessLevel _level;
        public MinPermissionAttribute(AccessLevel level)
        {
            _level = level;
        }

        public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IDependencyMap map)
        {
            var access = GetPermissions(context);
            return Task.FromResult(access >= _level ? PreconditionResult.FromSuccess() : PreconditionResult.FromError("Not Enough Permissions!"));
        }

        public AccessLevel GetPermissions(ICommandContext context)
        {
            if (context.User.IsBot) return AccessLevel.Blocked;

            var application = context.Client.GetApplicationInfoAsync().GetAwaiter().GetResult();

            if (application.Owner.Id == context.User.Id) return AccessLevel.BotOwner;

            var user = context.User as SocketGuildUser;
            if (user == null) return AccessLevel.IsPrivate;
            if (context.Guild.OwnerId == user.Id) return AccessLevel.ServerOwner;
            if (user.GuildPermissions.BanMembers || user.GuildPermissions.KickMembers) return AccessLevel.ServerModerator;
            //TRUSTED
            return AccessLevel.User;
        }
    }
}