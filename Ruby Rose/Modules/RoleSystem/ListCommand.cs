using Discord.Commands;
using MongoDB.Driver;
using RubyRose.Common;
using RubyRose.Common.Preconditions;
using RubyRose.Database;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RubyRose.Database.Models;

namespace RubyRose.Modules.RoleSystem
{
    [Name("Role System"), Group]
    public class ListCommand : ModuleBase
    {
        private readonly MongoClient _mongo;

        public ListCommand(IDependencyMap map)
        {
            _mongo = map?.Get<MongoClient>();
        }

        [Command("List")]
        [Summary("List all keyword for roles marked as Joinable")]
        [MinPermission(AccessLevel.User), RequireAllowed, Ratelimit(4, 1, Measure.Minutes)]
        public async Task List()
        {
            var sb = new StringBuilder();
            var joinables = await _mongo.GetCollection<Joinables>(Context.Client).GetListAsync(Context.Guild);

            sb.AppendLine("```");
            sb.AppendLine(joinables.Any(joinable => joinable.Level > 0)
                ? "Lv: 0 - All --Join all Lv 0"
                : "All --join all");
            foreach (var joinable in joinables.OrderBy(x => x.Level))
            {
                sb.AppendLine(joinables.Any(joina => joina.Level > 0)
                    ? $"Lv: {joinable.Level} - {joinable.Name.ToFirstUpper()}"
                    : $"{joinable.Name.ToFirstUpper()}");
            }
            sb.AppendLine("```");
            await Context.Channel.SendEmbedAsync(Embeds.Success("List of joinable Roles", sb.ToString()));
        }
    }
}