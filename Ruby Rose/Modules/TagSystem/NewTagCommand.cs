using Discord;
using Discord.Commands;
using MongoDB.Driver;
using NLog;
using RubyRose.Common;
using RubyRose.Common.Preconditions;
using RubyRose.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RubyRose.Database.Models;

namespace RubyRose.Modules.TagSystem
{
    [Name("Tag System"), Group]
    public class NewTagCommand : ModuleBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly MongoClient _mongo;

        public NewTagCommand(IDependencyMap map)
        {
            _mongo = map.Get<MongoClient>();
        }

        [Command("NewTag")]
        [MinPermission(AccessLevel.ServerModerator), RequireAllowed]
        public async Task NewTag(string name, [Remainder] string content)
        {
            name = name.ToLower();

            var allTags = _mongo.GetCollection<Tags>(Context.Client);
            var tags = await GetTagsAsync(allTags, Context.Guild);

            var newTag = new Tags
            {
                GuildId = Context.Guild.Id,
                Name = name,
                Content = content,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = Context.User.Id,
                LastUsed = DateTime.MinValue,
                Uses = 0
            };

            if (!tags.Exists(x => x.Name == name))
            {
                await allTags.InsertOneAsync(newTag);
                await ReplyAsync($"Tag `{name.ToFirstUpper()}` added to database!");
                Logger.Info($"New Tag {name} on {Context.Guild.Name}");
            }
            else
            {
                await ReplyAsync($"Tag {name.ToFirstUpper()} already existent");
                Logger.Warn($"Failed to add new Tag {name} to {Context.Guild.Name}, already Existent");
            }
        }

        [Command("NewTag")]
        [MinPermission(AccessLevel.ServerModerator), RequireAllowed]
        public async Task NewTag(string name, IAttachment attachment = null)
        {
            if (Context.Message.Attachments.Any())
            {
                name = name.ToLower();

                var allTags = _mongo.GetCollection<Tags>(Context.Client);
                var tags = await GetTagsAsync(allTags, Context.Guild);

                var newTag = new Tags
                {
                    GuildId = Context.Guild.Id,
                    Name = name,
                    Content = Context.Message.Attachments.First().Url,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = Context.User.Id,
                    LastUsed = DateTime.MinValue,
                    Uses = 0
                };

                if (!tags.Exists(x => x.Name == name))
                {
                    await allTags.InsertOneAsync(newTag);
                    await ReplyAsync($"Tag `{name.ToFirstUpper()}` added to database!");
                    Logger.Info($"New Tag {name} on {Context.Guild.Name}");
                }
                else
                {
                    await ReplyAsync($"Tag {name.ToFirstUpper()} already existent");
                    Logger.Warn($"Failed to add new Tag {name} to {Context.Guild.Name}, already Existent");
                }
            }
            else
                await Context.Channel.SendEmbedAsync(Embeds.UnmetPrecondition("Message contains no Attachment"));
        }

        private async Task<List<Tags>> GetTagsAsync(IMongoCollection<Tags> collection, IGuild guild)
        {
            var tagsCursor = await collection.FindAsync(f => f.GuildId == guild.Id);
            return await tagsCursor.ToListAsync();
        }
    }
}