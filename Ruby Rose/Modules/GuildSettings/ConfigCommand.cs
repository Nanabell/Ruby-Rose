using System;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using RubyRose.Common;
using RubyRose.Common.Preconditions;
using RubyRose.Database;
using RubyRose.Database.Models;

namespace RubyRose.Modules.GuildSettings
{
    [Name("Config")]
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