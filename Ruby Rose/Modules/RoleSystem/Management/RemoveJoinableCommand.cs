using Discord.Commands;
using MongoDB.Driver;
using RubyRose.Common;
using RubyRose.Common.Preconditions;
using RubyRose.Database;
using Serilog;
using System.Linq;
using System.Threading.Tasks;

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

            var c = _mongo.GetDiscordDb(Context.Client);
            var cGuild = await c.Find(g => g.Id == Context.Guild.Id).FirstAsync();

            if (cGuild.Joinable != null)
            {
                if (cGuild.Joinable.Any(j => j.Keyword == keyword))
                {
                    cGuild.Joinable.Remove(cGuild.Joinable.First(j => j.Keyword == keyword));
                    await c.UpdateOneAsync(f => f.Id == Context.Guild.Id, Builders<DatabaseModel>.Update.Set("Joinable", cGuild.Joinable));
                    Log.Information($"Dropped joinable {keyword} from Db");
                    await Context.Channel.SendEmbedAsync(Embeds.Success("Success", $"Dropped Joinable {keyword} from db"));
                }
                else
                {
                    Log.Error("Unable to Remove Joinable. Not Existent");
                    await Context.Channel.SendEmbedAsync(Embeds.NotFound($"Unable to find a Joinable Role with the keyword {keyword}."));
                }
            }
        }
    }
}