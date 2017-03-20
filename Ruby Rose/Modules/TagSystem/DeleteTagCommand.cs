using System;
using System.Threading.Tasks;
using Discord.Commands;
using MongoDB.Driver;
using RubyRose.Common;
using RubyRose.Common.Preconditions;
using Tag = RubyRose.MongoDB.Tag;

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
            var tscollec = _mongo.GetDatabase($"{Context.Guild.Id}").GetCollection<Tag>("TagSystem");
            var oldTag = await tscollec.FindOneAndDeleteAsync(f => f.TagName == keyWord);

            if (oldTag != null)
                await ReplyAsync($"Tag `{oldTag.TagName.ToFirstUpper()}` successfully Deleted!");
            else await Context.Channel.SendEmbedAsync(Embeds.NotFound($"Could not find a Tag with the name {keyWord}"));
        }
    }
}