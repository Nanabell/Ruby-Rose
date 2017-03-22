using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using MongoDB.Driver;
using RubyRose.Common;
using RubyRose.Common.Preconditions;
using Tag = RubyRose.MongoDB.Tag;

namespace RubyRose.Modules.TagSystem
{
    [Name("Tag System"), Group]
    public class TagCommand : ModuleBase
    {
        private readonly MongoClient _mongo;

        public TagCommand(IDependencyMap map)
        {
            if (map == null) throw new ArgumentNullException(nameof(map));
            _mongo = map.Get<MongoClient>();
        }

        [Command("Tag"), Alias("Tags")]
        [Summary("Display all Tags that exist")]
        [MinPermission(AccessLevel.User), RequireAllowed]
        public async Task Tag()
        {
            var sb = new StringBuilder();
            var tscollec = _mongo.GetDatabase($"{Context.Guild.Id}").GetCollection<Tag>("TagSystem");
            var tags = tscollec.Find("{}").ToList();
            sb.AppendLine("**Tags**:");
            sb.AppendLine(string.Join(" ", tags.Select(x => $"`{x.TagName}`")));
            await ReplyAsync($"{sb}");
        }

        [Command("Tag")]
        [Summary("Use simply keywords to display a custom set response")]
        [MinPermission(AccessLevel.User), RequireAllowed]
        public async Task Tag([Remainder] string keyWord)
        {
            keyWord = keyWord.ToLower();
            var tscollec = _mongo.GetDatabase($"{Context.Guild.Id}").GetCollection<Tag>("TagSystem");
            var tag = tscollec.Find(f => f.TagName == keyWord).FirstOrDefault();

            if (tag != null)
            {
                var update = Builders<Tag>.Update
                    .Set("Uses", tag.Uses + 1);
                await tscollec.UpdateOneAsync(f => f.Id == tag.Id, update);
                await ReplyAsync(tag.Response);
            }
            else
                await Context.Channel.SendEmbedAsync(
                    Embeds.NotFound($"A Tag with the name {keyWord.ToFirstUpper()} was not found!"));
        }
    }
}