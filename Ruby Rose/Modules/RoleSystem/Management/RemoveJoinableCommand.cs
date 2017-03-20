using System.Threading.Tasks;
using Discord.Commands;
using MongoDB.Driver;
using RubyRose.Common;
using RubyRose.Common.Preconditions;
using RubyRose.Modules.RoleSystem.Db;

namespace RubyRose.Modules.RoleSystem.Management
{
    [Name("Role System Management"), Group]
    public class RemoveJoinableCommand : ModuleBase
    {
        private readonly MongoClient _mongo;

        public RemoveJoinableCommand(IDependencyMap map)
        {
            _mongo = map.Get<MongoClient>();
        }

        [Command("RemoveJoinable")]
        [Summary("Remove the marker from a marked Joinable role")]
        [MinPermission(AccessLevel.ServerModerator), RequireAllowed]
        public async Task RemoveJoinable(string keyword)
        {
            keyword = keyword.ToLower();
            var jrcollec = _mongo.GetDatabase($"{Context.Guild.Id}").GetCollection<JoinSystemSerializer>("JoinSystem");
            if (jrcollec.Find(f => f.Keyword == keyword).Any())
            {
                await jrcollec.FindOneAndDeleteAsync(f => f.Keyword == keyword);
                await Context.Channel.SendEmbedAsync(
                    Embeds.Success("Role no longer Joinable", "Joinable Role deleted from Database"));
            }
            else
            {
                await Context.Channel.SendEmbedAsync(
                    Embeds.NotFound($"Unable to find a Joinable Role with the keyword {keyword}."));
            }
        }
    }
}