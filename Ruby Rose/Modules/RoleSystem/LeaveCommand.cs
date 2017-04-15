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
using RubyRose.Database.Models;

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
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task Leave([Remainder] string name)
        {
            var roles = new List<IRole>();
            var sb = new StringBuilder();

            name = name.ToLower();
            var joinables = await _mongo.GetCollection<Joinables>(Context.Client).GetListAsync(Context.Guild);

            foreach (var word in name.Split(' '))
            {
                if (word == "all")
                {
                    joinables.ForEach(x =>
                    {
                        if (((IGuildUser) Context.User).RoleIds.Contains(x.RoleId))
                        {
                            roles.Add(Context.Guild.GetRole(x.RoleId));
                            sb.AppendLine(x.Name.ToFirstUpper());
                        }
                        else sb.AppendLine($"{x.Name.ToFirstUpper()} --already left");
                    });
                    break;
                }
                var result = joinables.FirstOrDefault(f => f.Name == word);
                if (result == null) continue;

                if (!((IGuildUser) Context.User).RoleIds.Contains(result.RoleId))
                {
                    sb.AppendLine($"{result.Name.ToFirstUpper()} --already left");
                    continue;
                }
                roles.Add(Context.Guild.GetRole(result.RoleId));
                sb.AppendLine(result.Name.ToFirstUpper());
            }

            if (roles.Count > 0)
            {
                await ((SocketGuildUser) Context.User).RemoveRolesAsync(roles);
                await Context.Channel.SendEmbedAsync(Embeds.Success("You left the roles for", sb.ToString()));
            }
            else
            {
                await Context.Channel.SendEmbedAsync(Embeds.NotFound("No valid role with given input found.\n" + sb));
            }
        }
    }
}