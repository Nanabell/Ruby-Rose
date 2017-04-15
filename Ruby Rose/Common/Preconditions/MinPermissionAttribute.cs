using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using MongoDB.Driver;
using RubyRose.Database;
using RubyRose.Database.Models;

namespace RubyRose.Common.Preconditions
{
    public enum AccessLevel
    {
        Blocked,
        IsPrivate,
        User,
        ServerModerator,
        ServerAdmin,
        ServerOwner,
        Special,
        BotOwner
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class MinPermissionAttribute : PreconditionAttribute
    {
        private readonly AccessLevel _level;
        private static MongoClient _mongo;

        public MinPermissionAttribute(AccessLevel level)
        {
            _level = level;
        }

        public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IDependencyMap map)
        {
            _mongo = map.Get<MongoClient>();
            var access = GetPermissionsAsync(_mongo, context, _level);

            return _level == AccessLevel.Special
                ? Task.FromResult(access >= _level
                    ? PreconditionResult.FromSuccess()
                    : PreconditionResult.FromError("This command needs special permissions!"))
                : Task.FromResult(access >= _level
                    ? PreconditionResult.FromSuccess()
                    : PreconditionResult.FromError("Not enough permissions!"));
        }

        public AccessLevel GetPermissionsAsync(MongoClient mongo, ICommandContext context, AccessLevel requestedLvl)
        {
            if (context.User.IsBot)
                return AccessLevel.Blocked;

            var application = context.Client.GetApplicationInfoAsync().GetAwaiter().GetResult();

            if (application.Owner.Id == context.User.Id)
                return AccessLevel.BotOwner;

            var user = context.User as SocketGuildUser;
            if (user == null)
                return AccessLevel.IsPrivate;

            if (requestedLvl == AccessLevel.Special)
            {
                var users = _mongo.GetCollection<Users>(context.Client).GetListAsync(context.Guild).GetAwaiter().GetResult();

                if (users.Exists(x => x.UserId == user.Id))
                {
                    var cUser = users.First(u => u.UserId == user.Id);
                    if (cUser.IsSpecial)
                        return AccessLevel.Special;
                }
            }

            if (context.Guild.OwnerId == user.Id)
                return AccessLevel.ServerOwner;
            if (user.GuildPermissions.BanMembers)
                return AccessLevel.ServerAdmin;

            return user.GuildPermissions.KickMembers ? AccessLevel.ServerModerator : AccessLevel.User;
        }
    }
}