using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MongoDB.Driver;
using NLog;
using RubyRose.Common;
using RubyRose.Common.Preconditions;
using RubyRose.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RubyRose.Database.Models;

namespace RubyRose.Modules.RoleSystem
{
    [Name("Role System"), Group]
    public class JoinCommand : ModuleBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly MongoClient _mongo;

        public JoinCommand(IDependencyMap map)
        {
            _mongo = map.Get<MongoClient>();
        }

        [Command("Join")]
        [Summary("Join roles that are marked as joinable.")]
        [MinPermission(AccessLevel.User), RequireAllowed, Ratelimit(4, 1, Measure.Minutes)]
        public async Task Join([Remainder] string input)
        {
            var joinables = await _mongo.GetCollection<Joinables>(Context.Client).GetListAsync(Context.Guild);
            var names = input.ToLower().Split(' ');

            var entitled = GetEntitled(joinables, names, Context.User as IGuildUser);

            if (entitled.Any())
            {
                var sb = new StringBuilder();
                await ((SocketGuildUser) Context.User).AddRolesAsync(entitled, new RequestOptions { RetryMode = RetryMode.AlwaysRetry });
                foreach (var role in entitled)
                {
                    sb.AppendLine(role.Name);
                }
                await Context.Channel.SendEmbedAsync(Embeds.Success("Added to", sb.ToString()));
            }
            else
            {
                await Context.ReplyAsync(
                    ":warning: Unable to add to any Role. Either invalid input or you already have them");
            }
        }

        private static ICollection<IRole> GetEntitled(ICollection<Joinables> joinables, ICollection<string> names, IGuildUser user)
        {
            var entitled = new List<IRole>();
            var userRoles = user.GetRoles().ToList();
            var guild = user.Guild;

            if (names.Contains("all"))
                return GetAllEntitled(joinables, user);

            foreach (var name in names)
            {
                var joinable = joinables.FirstOrDefault(join => join.Name == name);

                if (userRoles.Any(role => role.Id == joinable.RoleId))
                    continue;
                if (joinable == null) continue;

                var sameLevel = joinables.Where(join => join.Level == joinable.Level);
                var skip = false;
                foreach (var join in sameLevel)
                {
                    if (userRoles.Any(role => role.Id == join.RoleId))
                        skip = true;
                }
                if (skip)
                    continue;

                try
                {
                    entitled.Add(guild.GetRole(joinable.RoleId));
                }
                catch (Exception e)
                {
                    Logger.Warn(e, "Failed to add Role to entitled RoleList");
                }
            }
            return entitled;
        }

        private static ICollection<IRole> GetAllEntitled(IEnumerable<Joinables> joinables, IGuildUser user)
        {
            var entitled = new List<IRole>();
            var userRoles = user.GetRoles().ToList();
            var guild = user.Guild;

            foreach (var joinable in joinables)
            {
                if (userRoles.Any(role => role.Id == joinable.RoleId))
                    continue;
                if (joinable.Level > 0)
                    continue;

                try
                {
                    entitled.Add(guild.GetRole(joinable.RoleId));
                }
                catch (Exception e)
                {
                    Logger.Warn(e, "Failed to add Role to entitled RoleList");
                }
            }
            return entitled;
        }
    }
}