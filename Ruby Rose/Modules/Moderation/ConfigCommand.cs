using Discord.Commands;
using MongoDB.Driver;
using RubyRose.Database;
using System;
using System.Threading.Tasks;
using RubyRose.Common;
using RubyRose.Common.Preconditions;
using RubyRose.Database.Models;
using Microsoft.Extensions.DependencyInjection;

namespace RubyRose.Modules.Moderation
{
    [Name("Moderation")]
    public class ConfigCommand : ModuleBase
    {
        private readonly MongoClient _mongo;

        public ConfigCommand(IServiceProvider provider)
        {
            _mongo = provider.GetService<MongoClient>();
        }

        [Command("Config")]
        [MinPermission(AccessLevel.ServerModerator)]
        public async Task Config()
        {
            var settings = await _mongo.GetCollection<Settings>(Context.Client).GetByGuildAsync(Context.Guild.Id);

            await Context.ReplyAsync($"Current Settings for {Context.Guild.Name}\n{settings}");
        }
    }
}