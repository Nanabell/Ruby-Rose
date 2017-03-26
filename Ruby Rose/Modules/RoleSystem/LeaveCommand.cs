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
            var c = _mongo.GetDiscordDb(Context.Client);
            var cGuild = await c.Find(g => g.Id == Context.Guild.Id).FirstAsync();

            if (cGuild.Joinable != null)
            {
                foreach (var word in keyword.Split(' '))
                {
                    if (word == "all")
                    {
                        cGuild.Joinable.ForEach(x =>
                        {
                            if ((Context.User as SocketGuildUser).Roles.Contains(Context.Guild.GetRole(x.Role.Id)))
                            {
                                roles.Add(Context.Guild.GetRole(x.Role.Id));
                                sb.AppendLine(x.Keyword.ToFirstUpper());
                            }
                            else sb.AppendLine($"{x.Keyword.ToFirstUpper()} --already left");
                        });
                        break;
                    }
                    var result = cGuild.Joinable.FirstOrDefault(f => f.Keyword == word);
                    if (result == null) continue;

                    if (!(Context.User as SocketGuildUser).Roles.Contains(Context.Guild.GetRole(result.Role.Id)))
                    {
                        sb.AppendLine($"{result.Keyword.ToFirstUpper()} --already left");
                        continue;
                    }
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
                    await Context.Channel.SendEmbedAsync(Embeds.NotFound("No valid role with given input found.\n" + sb.ToString()));
                }
            }
        }
    }
}