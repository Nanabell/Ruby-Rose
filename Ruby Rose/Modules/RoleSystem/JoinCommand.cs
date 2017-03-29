using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MongoDB.Driver;
using RubyRose.Common;
using RubyRose.Common.Preconditions;
using Serilog;
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
        private readonly MongoClient _mongo;

        public JoinCommand(IDependencyMap map)
        {
            _mongo = map.Get<MongoClient>();
        }

        [Command("Join")]
        [Summary("Join roles that are marked as joinable.")]
        [MinPermission(AccessLevel.User), RequireAllowed, Ratelimit(4, 1, Measure.Minutes)]
        public async Task Join([Remainder] string keyword)
        {
            var roles = new List<IRole>();
            var sb = new StringBuilder();

            keyword = keyword.ToLower();
            var c = _mongo.GetDiscordDb(Context.Client);
            var cGuild = await c.Find(g => g.Id == Context.Guild.Id).FirstAsync();

            if (cGuild.Joinable != null)
            {
                foreach (var word in keyword.Split(' '))
                {
                    if (word == "all")
                    {
                        foreach (var join in cGuild.Joinable)
                        {
                            if (join.Level > 0)
                            {
                                sb.AppendLine($"{join.Keyword.ToFirstUpper()} --cant join with all");
                                continue;
                            }
                            if (!(Context.User as SocketGuildUser).Roles.Contains(Context.Guild.GetRole(join.Role.Id)))
                            {
                                roles.Add(Context.Guild.GetRole(join.Role.Id));
                                sb.AppendLine(join.Keyword.ToFirstUpper());
                            }
                            else sb.AppendLine($"{join.Keyword.ToFirstUpper()} --already joined");
                        };
                        break;
                    }
                    var result = cGuild.Joinable.FirstOrDefault(f => f.Keyword == word);
                    if (result == null) continue;
                    try
                    {
                        if (result.Level > 0)
                        {
                            var skip = false;
                            var rolelist = cGuild.Joinable.Where(x => x.Level == result.Level).ToList();
                            var userRoles = (Context.User as SocketGuildUser).Roles;
                            foreach (var role in rolelist)
                            {
                                if (userRoles.Any(x => x.Id == role.Role.Id))
                                {
                                    sb.AppendLine($"{result.Keyword.ToFirstUpper()} --cant join, user has already a role of level {result.Level}");
                                    skip = true;
                                }
                            }
                            if (skip)
                                continue;
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error($"Role Level Compare Faild:\n{e}");
                    }

                    if ((Context.User as SocketGuildUser).Roles.Contains(Context.Guild.GetRole(result.Role.Id)))
                    {
                        sb.AppendLine($"{result.Keyword.ToFirstUpper()} --already joined");
                        continue;
                    }
                    roles.Add(Context.Guild.GetRole(result.Role.Id));
                    sb.AppendLine(result.Keyword.ToFirstUpper());
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
}