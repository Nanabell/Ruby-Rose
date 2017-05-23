using Discord;
using Discord.Commands;
using MongoDB.Driver;
using NLog;
using RubyRose.Common;
using RubyRose.Common.Preconditions;
using RubyRose.Database;
using System.Threading.Tasks;
using RubyRose.Database.Models;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace RubyRose.Modules.TagSystem
{
    [Name("Tag System"), Group]
    public class DeleteTagCommand : ModuleBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly MongoClient _mongo;

        public DeleteTagCommand(IServiceProvider provider)
        {
            _mongo = provider.GetService<MongoClient>();
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
                Logger.Info($"Deleted Tag {name} on {Context.Guild.Name}");
            }
            else
            {
                await ReplyAsync($"Tag `{name.ToFirstUpper()}` not Existent");
                Logger.Warn($"Failed to delete Tag {name} on {Context.Guild.Name}, not Existent");
            }
        }

        private static async Task<Tags> GetTagAsync(IMongoCollection<Tags> collection, IGuild guild, string name)
        {
            var tagsCursors = await collection.FindAsync(f => f.GuildId == guild.Id && f.Name == name);
            return await tagsCursors.FirstOrDefaultAsync();
        }
    }
}