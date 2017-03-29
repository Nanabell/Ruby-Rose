using Discord.Commands;
using MongoDB.Driver;
using RubyRose.Common;
using RubyRose.Common.Preconditions;
using RubyRose.Database;
using System.Linq;
using Serilog;
using System;
using System.Text;
using System.Threading.Tasks;

namespace RubyRose.Modules.RoleSystem
{
    [Name("Role System"), Group]
    public class ListCommand : ModuleBase
    {
        private readonly MongoClient _mongo;

        public ListCommand(IDependencyMap map)
        {
            if (map == null) throw new ArgumentNullException(nameof(map));
            _mongo = map.Get<MongoClient>();
        }

        [Command("List")]
        [Summary("List all keyword for roles marked as Joinable")]
        [MinPermission(AccessLevel.User), RequireAllowed, Ratelimit(4, 1, Measure.Minutes)]
        public async Task List()
        {
            var sb = new StringBuilder();
            var c = _mongo.GetDiscordDb(Context.Client);
            var cGuild = await c.Find(g => g.Id == Context.Guild.Id).FirstOrDefaultAsync();

            if (cGuild.Joinable != null)
            {
                sb.AppendLine("```");
                sb.AppendLine("Lv: 0 - All");
                foreach (var join in cGuild.Joinable.OrderBy(x => x.Level))
                {
                    sb.AppendLine($"Lv: {join.Level} - {join.Keyword.ToFirstUpper()}");
                }
                sb.AppendLine("```");
                await Context.Channel.SendEmbedAsync(Embeds.Success("list of keyword to join a role", sb.ToString()));
            }
            else
            {
                Log.Error("Joinables has no Roles yet");
            }
        }
    }
}