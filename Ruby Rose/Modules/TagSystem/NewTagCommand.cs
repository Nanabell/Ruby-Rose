using System;
using System.Threading.Tasks;
using Discord.Commands;
using MongoDB.Driver;
using RubyRose.Common;
using RubyRose.Common.Preconditions;
using Tag = RubyRose.MongoDB.Tag;
using RubyRose.Database;
using System.Linq;
using Serilog;
using Discord;

namespace RubyRose.Modules.TagSystem
{
    [Name("Tag System"), Group]
    public class NewTagCommand : ModuleBase
    {
        private readonly MongoClient _mongo;

        public NewTagCommand(IDependencyMap map)
        {
            _mongo = map.Get<MongoClient>();
        }

        [Command("NewTag")]
        [Summary("Creates a new Tag")]
        [MinPermission(AccessLevel.ServerModerator), RequireAllowed]
        public async Task NewTag(string keyWord, [Remainder] string response)
        {
            keyWord = keyWord.ToLower();

            var c = _mongo.GetDiscordDb(Context.Client);
            var cGuild = await c.Find(g => g.Id == Context.Guild.Id).FirstAsync();

            var newTag = new Tags
            {
                Name = keyWord,
                Response = response,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = Context.User.Id,
                LastUsed = DateTime.MinValue,
                Uses = 0
            };

            if (cGuild.Tag != null)
            {
                if (!cGuild.Tag.Any(t => t.Name == keyWord))
                {
                    cGuild.Tag.Add(newTag);
                    await c.UpdateOneAsync(g => g.Id == Context.Guild.Id, Builders<DatabaseModel>.Update.Set("Tag", cGuild.Tag));
                }
                else
                {
                    Log.Error($"Failed to add Tag {keyWord} on {Context.Guild.Name}. Already existent");
                    await Context.Channel.SendEmbedAsync(Embeds.Invalid($"Can't create Tag {keyWord} since it is already existent"));
                    return;
                }
            }
            else
            {
                await c.UpdateOneAsync(g => g.Id == Context.Guild.Id, Builders<DatabaseModel>.Update.AddToSet("Tag", newTag));
            }
            Log.Information($"Added new Tag {newTag.Name} on {Context.Guild.Name}");
            await ReplyAsync($"Tag `{keyWord.ToFirstUpper()}` successfully added!");
        }

        [Command("NewTag")]
        [Summary("Creates a new Tag")]
        [MinPermission(AccessLevel.ServerModerator), RequireAllowed]
        public async Task NewTag(string keyWord, IAttachment attachment)
        {
            keyWord = keyWord.ToLower();

            var c = _mongo.GetDiscordDb(Context.Client);
            var cGuild = await c.Find(g => g.Id == Context.Guild.Id).FirstAsync();

            var newTag = new Tags
            {
                Name = keyWord,
                Response = attachment.Url,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = Context.User.Id,
                LastUsed = DateTime.MinValue,
                Uses = 0
            };

            if (cGuild.Tag != null)
            {
                if (!cGuild.Tag.Any(t => t.Name == keyWord))
                {
                    cGuild.Tag.Add(newTag);
                    await c.UpdateOneAsync(g => g.Id == Context.Guild.Id, Builders<DatabaseModel>.Update.Set("Tag", cGuild.Tag));
                }
                else
                {
                    Log.Error($"Failed to add Tag {keyWord} on {Context.Guild.Name}. Already existent");
                    await Context.Channel.SendEmbedAsync(Embeds.Invalid($"Can't create Tag {keyWord} since it is already existent"));
                    return;
                }
            }
            else
            {
                await c.UpdateOneAsync(g => g.Id == Context.Guild.Id, Builders<DatabaseModel>.Update.AddToSet("Tag", newTag));
            }
            Log.Information($"Added new Tag {newTag.Name} on {Context.Guild.Name}");
            await ReplyAsync($"Tag `{keyWord.ToFirstUpper()}` successfully added!");
        }
    }
}