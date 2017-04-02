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

namespace RubyRose.Modules.RoleSystem
{
    [Name("Role System"), Group]
    public class JoinCommand : ModuleBase
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private readonly MongoClient _mongo;

        public JoinCommand(IDependencyMap map)
        {
            _mongo = map.Get<MongoClient>();
        }

        [Command("Join")]
        [Summary("Join roles that are marked as joinable.")]
        [MinPermission(AccessLevel.User), RequireAllowed, Ratelimit(4, 1, Measure.Minutes)]
        public async Task Join([Remainder] string name)
        {
            var roles = new List<IRole>();
            var sb = new StringBuilder();

            name = name.ToLower();
            var joinables = await _mongo.GetCollection<Joinables>(Context.Client).GetListAsync(Context.Guild);

            foreach (var word in name.Split(' '))
            {
                if (word == "all")
                {
                    foreach (var joinable in joinables)
                    {
                        if (joinable.Level > 0)
                        {
                            sb.AppendLine($"{joinable.Name.ToFirstUpper()} --cant join with all");
                            continue;
                        }
                        if (!(Context.User as IGuildUser).RoleIds.Contains(joinable.RoleId))
                        {
                            roles.Add(Context.Guild.GetRole(joinable.RoleId));
                            sb.AppendLine(joinable.Name.ToFirstUpper());
                        }
                        else sb.AppendLine($"{joinable.Name.ToFirstUpper()} --already joined");
                    };
                    break;
                }
                var result = joinables.FirstOrDefault(f => f.Name == word);
                if (result == null) continue;
                try
                {
                    if (result.Level > 0)
                    {
                        var skip = false;
                        var rolelist = joinables.Where(x => x.Level == result.Level).ToList();
                        var userRoles = (Context.User as SocketGuildUser).Roles;
                        foreach (var role in rolelist)
                        {
                            if (userRoles.Any(x => x.Id == role.RoleId))
                            {
                                sb.AppendLine($"{result.Name.ToFirstUpper()} --cant join, user has already a role of level {result.Level}");
                                skip = true;
                            }
                        }
                        if (skip)
                            continue;
                    }
                }
                catch (Exception e)
                {
                    logger.Warn($"Role Level Compare Faild:\n{e}");
                }

                if ((Context.User as IGuildUser).RoleIds.Contains(result.RoleId))
                {
                    sb.AppendLine($"{result.Name.ToFirstUpper()} --already joined");
                    continue;
                }
                roles.Add(Context.Guild.GetRole(result.RoleId));
                sb.AppendLine(result.Name.ToFirstUpper());
            }

            if (roles.Count > 0)
            {
                await (Context.User as SocketGuildUser).AddRolesAsync(roles, new RequestOptions { RetryMode = RetryMode.AlwaysRetry });
                await Context.Channel.SendEmbedAsync(Embeds.Success("You now have the roles for", sb.ToString()));
            }
            else
            {
                await Context.Channel.SendEmbedAsync(Embeds.NotFound("No valid role with given input found.\n" + sb.ToString()));
            }
        }
    }
}