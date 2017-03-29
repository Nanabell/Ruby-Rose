using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using MongoDB.Driver;
using RubyRose.Common;
using RubyRose.Common.Preconditions;
using Tag = RubyRose.MongoDB.Tag;
using Serilog;
using RubyRose.Database;

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
            var c = _mongo.GetDiscordDb(Context.Client);
            var cGuild = await c.Find(g => g.Id == Context.Guild.Id).FirstAsync();
            if (cGuild.Tag != null)
            {
                sb.AppendLine("**Tags**:");
                sb.AppendLine(string.Join(" ", cGuild.Tag.OrderBy(x => x.Name).Select(x => $"`{x.Name}`")));
                await ReplyAsync($"{sb}");
            }
            else
            {
                Log.Error($"Faild to load Tags on {Context.Guild.Name}. None Existent");
                await ReplyAsync("Failed to load Tags.\nReason: None existing");
            }
        }

        [Command("Tag")]
        [Summary("Use simply keywords to display a custom set response")]
        [MinPermission(AccessLevel.User), RequireAllowed]
        public async Task Tag([Remainder] string keyWord)
        {
            keyWord = keyWord.ToLower();
            var c = _mongo.GetDiscordDb(Context.Client);
            var cGuild = await c.Find(g => g.Id == Context.Guild.Id).FirstAsync();

            if (cGuild.Tag != null)
            {
                var tag = cGuild.Tag.FirstOrDefault(t => t.Name == keyWord);

                if (tag != null)
                {
                    cGuild.Tag[cGuild.Tag.IndexOf(tag)].Uses++;
                    cGuild.Tag[cGuild.Tag.IndexOf(tag)].LastUsed = DateTime.UtcNow;
                    await c.UpdateOneAsync(g => g.Id == Context.Guild.Id, Builders<DatabaseModel>.Update.Set("Tag", cGuild.Tag));
                    await ReplyAsync(tag.Response);
                }
                else
                {
                    Log.Error($"Unable to find Tag with Name {keyWord} on {Context.Guild.Name}");
                    await ReplyAsync($"Tag with the Name {keyWord} not found.");
                }
            }
            else
            {
                Log.Error($"Faild to load Tags on {Context.Guild.Name}. None Existent");
                await ReplyAsync("Failed to load Tags.\nReason: None existing");
            }
        }
    }
}