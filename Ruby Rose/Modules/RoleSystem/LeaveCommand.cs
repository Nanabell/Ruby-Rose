using System;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MongoDB.Driver;
using RubyRose.Common;
using RubyRose.Common.Preconditions;
using RubyRose.Database;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using RubyRose.Database.Models;
using Microsoft.Extensions.DependencyInjection;

namespace RubyRose.Modules.RoleSystem
{
    [Name("Role System"), Group]
    public class LeaveCommand : ModuleBase
    {
        private readonly MongoClient _mongo;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public LeaveCommand(IServiceProvider provider)
        {
            _mongo = provider.GetService<MongoClient>();
        }

        [Command("Leave")]
        [Summary("Leavea role marked as Joinable")]
        [MinPermission(AccessLevel.User), RequireAllowed, Ratelimit(4, 1, Measure.Minutes)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task Leave([Remainder] string input)
        {
            var joinables = await _mongo.GetCollection<Joinables>(Context.Client).GetListAsync(Context.Guild);
            var names = input.ToLower().Split(' ');

            var entitled = GetEntitled(joinables, names, Context.User as IGuildUser);

            if (entitled.Any())
            {
                var sb = new StringBuilder();
                await ((SocketGuildUser)Context.User).RemoveRolesAsync(entitled, new RequestOptions { RetryMode = RetryMode.AlwaysRetry });
                foreach (var role in entitled)
                {
                    sb.AppendLine(role.Name);
                }
                await Context.Channel.SendEmbedAsync(Embeds.Success("Removed from", sb.ToString()));
            }
            else
            {
                await Context.ReplyAsync(
                    ":warning: Unable to remove from any Role. Either invalid input or you dont have them");
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

                if (userRoles.All(role => role.Id != joinable.RoleId))
                    continue;
                if (joinable == null) continue;

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
                if (userRoles.All(role => role.Id != joinable.RoleId))
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