using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MongoDB.Driver;
using RubyRose.Common;
using RubyRose.Common.Preconditions;
using RubyRose.Modules.RoleSystem.Db;
using System.Linq;

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
            var jrcollec = _mongo.GetDatabase($"{Context.Guild.Id}").GetCollection<JoinSystemSerializer>("JoinSystem");

            foreach (var word in keyword.Split(' '))
            {
                if (word == "all")
                {
                    var all = jrcollec.Find("{}");
                    await all.ForEachAsync(x =>
                    {
                        if (!(Context.User as SocketGuildUser).Roles.Contains(Context.Guild.GetRole(x.Role.Id)))
                        {
                            roles.Add(Context.Guild.GetRole(x.Role.Id));
                            sb.AppendLine(x.Keyword.ToFirstUpper());
                        }
                    });
                    break;
                }
                var result = jrcollec.Find(f => f.Keyword == word).FirstOrDefault();
                if (result == null) continue;

                if ((Context.User as SocketGuildUser).Roles.Contains(Context.Guild.GetRole(result.Role.Id))) continue;
                roles.Add(Context.Guild.GetRole(result.Role.Id));
                sb.AppendLine(result.Keyword.ToFirstUpper());
            }

            if (roles.Count > 0)
            {
                Console.WriteLine(sb.ToString());
                roles.ForEach(x => { Console.WriteLine(x.Name); });
                await (Context.User as SocketGuildUser).AddRolesAsync(roles, new RequestOptions { RetryMode = RetryMode.AlwaysRetry });
                await Context.Channel.SendEmbedAsync(Embeds.Success("You now have the roles for", sb.ToString()));
            }
            else
            {
                await Context.Channel.SendEmbedAsync(Embeds.NotFound("No valid role with given input found."));
            }
        }
    }
}