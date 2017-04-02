using Discord;
using Discord.Commands;
using MongoDB.Driver;
using NLog;
using RubyRose.Common;
using RubyRose.Common.Preconditions;
using RubyRose.Database;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RubyRose.Modules.TagSystem
{
    [Name("Tag System"), Group]
    public class DeleteTagCommand : ModuleBase
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private readonly MongoClient _mongo;

        public DeleteTagCommand(IDependencyMap map)
        {
            _mongo = map.Get<MongoClient>();
        }

        [Command("DeleteTag"), Alias("RemoveTag", "RmTag")]
        [Summary("Delete a Tag from the Database")]
        [MinPermission(AccessLevel.ServerModerator), RequireAllowed]
        public async Task DeleteTag(string name)
        {
            name = name.ToLower();
            var allTags = _mongo.GetCollection<Tags>(Context.Client);
            var tag = await GetTagAsync(allTags, Context.Guild, name);

            if (tag != null)
            {
                await allTags.DeleteAsync(tag);
                await ReplyAsync($"Tag `{name.ToFirstUpper()}` dropped from Database");
                logger.Info($"Deleted Tag {name} on {Context.Guild.Name}");
            }
            else
            {
                await ReplyAsync($"Tag `{name.ToFirstUpper()}` not Existent");
                logger.Warn($"Failed to delete Tag {name} on {Context.Guild.Name}, not Existent");
            }
        }

        private async Task<Tags> GetTagAsync(IMongoCollection<Tags> collection, IGuild guild, string name)
        {
            var TagsCursors = await collection.FindAsync(f => f.GuildId == guild.Id && f.Name == name);
            return await TagsCursors.FirstOrDefaultAsync();
        }
    }
}