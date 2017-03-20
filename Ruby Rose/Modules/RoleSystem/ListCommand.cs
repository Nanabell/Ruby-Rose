using System;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using MongoDB.Driver;
using RubyRose.Common;
using RubyRose.Common.Preconditions;
using RubyRose.Modules.RoleSystem.Db;

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
            var jrcollec = _mongo.GetDatabase($"{Context.Guild.Id}").GetCollection<JoinSystemSerializer>("JoinSystem");
            var all = await jrcollec.FindAsync("{}");
            sb.AppendLine("all");
            await all.ForEachAsync(x => sb.AppendLine(x.Keyword));
            await Context.Channel.SendEmbedAsync(Embeds.Success("list of keyword to join a role", sb.ToString()));
        }
    }
}