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
    public class NewTagCommand :ModuleBase
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

            var tscollec = _mongo.GetDatabase($"{Context.Guild.Id}").GetCollection<Tag>("TagSystem");
            var results = tscollec.Find(f => f.TagName == keyWord).FirstOrDefault();

            if (results == null)
            {
                var newTag = new Tag
                {
                    TagName = keyWord,
                    Response = response,
                    Creator = Context.User.Id,
                    CreatedAt = DateTime.UtcNow,
                    Uses = 0
                };

                await tscollec.InsertOneAsync(newTag);
                await ReplyAsync($"Tag `{keyWord.ToFirstUpper()}` successfully added!");
            }
            else await Context.Channel.SendEmbedAsync(Embeds.Invalid($"Can't create Tag {keyWord} since it is already existent"));
        }
    }
}