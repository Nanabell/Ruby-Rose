using Discord.Commands;
using MongoDB.Driver;
using RubyRose.Common;
using RubyRose.Common.Preconditions;
using RubyRose.Database;
using Serilog;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RubyRose.Modules.TagSystem
{
    [Name("Tag System"), Group]
    public class DeleteTagCommand : ModuleBase
    {
        private readonly MongoClient _mongo;

        public DeleteTagCommand(IDependencyMap map)
        {
            if (map == null) throw new ArgumentNullException(nameof(map));
            _mongo = map.Get<MongoClient>();
        }

        [Command("DeleteTag"), Alias("RemoveTag", "RmTag")]
        [Summary("Delete a Tag from the Database")]
        [MinPermission(AccessLevel.ServerModerator), RequireAllowed]
        public async Task DeleteTag(string keyWord)
        {
            keyWord = keyWord.ToLower();
            var c = _mongo.GetDiscordDb(Context.Client);
            var cGuild = await c.Find(g => g.Id == Context.Guild.Id).FirstAsync();

            if (cGuild.Tag != null)
            {
                var tag = cGuild.Tag.First(t => t.Name == keyWord);
                cGuild.Tag.Remove(tag);
                await c.UpdateOneAsync(g => g.Id == Context.Guild.Id, Builders<DatabaseModel>.Update.Set("Tag", cGuild.Tag));
                Log.Information($"Dropped Tag {tag.Name} on {Context.Guild.Name} from Db");
                await ReplyAsync($"Tag `{tag.Name.ToFirstUpper()}` successfully Deleted!");
            }
            else
            {
                Log.Error($"Unable to delete Tag {keyWord} on {Context.Guild.Name}. Not Existent in Db");
                await Context.Channel.SendEmbedAsync(Embeds.NotFound($"Could not find a Tag with the name {keyWord}"));
            }
        }
    }
}