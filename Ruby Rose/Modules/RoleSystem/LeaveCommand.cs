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
    public class LeaveCommand : ModuleBase
    {
        private readonly MongoClient _mongo;

        public LeaveCommand(IDependencyMap map)
        {
            _mongo = map.Get<MongoClient>();
        }

        [Command("Leave")]
        [Summary("Leavea role marked as Joinable")]
        [MinPermission(AccessLevel.User), RequireAllowed, Ratelimit(4, 1, Measure.Minutes)]
        public async Task Leave([Remainder] string keyword)
        {
            var roles = new List<IRole>();
            var sb = new StringBuilder();

            keyword = keyword.ToLower();
            var jrcollec = _mongo.GetDatabase($"{Context.Guild.Id}").GetCollection<JoinSystemSerializer>("JoinSystem");

            foreach (var word in keyword.Split(' '))
            {
                if (word == "all")
                {
                    var all = await jrcollec.FindAsync("{}");
                    await all.ForEachAsync(x =>
                    {
                        if ((Context.User as SocketGuildUser).Roles.Contains(Context.Guild.GetRole(x.Role.Id)))
                        {
                            roles.Add(Context.Guild.GetRole(x.Role.Id));
                            sb.AppendLine(x.Keyword.ToFirstUpper());
                        }
                    });
                    break;
                }
                var result = await jrcollec.Find(f => f.Keyword == word).FirstOrDefaultAsync();
                if (result == null) continue;

                if (!(Context.User as SocketGuildUser).Roles.Contains(Context.Guild.GetRole(result.Role.Id))) continue;
                roles.Add(Context.Guild.GetRole(result.Role.Id));
                sb.AppendLine(result.Keyword.ToFirstUpper());
            }

            if (roles.Count > 0)
            {
                await (Context.User as SocketGuildUser).RemoveRolesAsync(roles);
                await Context.Channel.SendEmbedAsync(Embeds.Success("You left the roles for", sb.ToString()));
            }
            else
            {
                await Context.Channel.SendEmbedAsync(Embeds.NotFound("No valid role with given input found."));
            }
        }
    }
}