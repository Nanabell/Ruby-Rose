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
    public class RwbySleeper : ModuleBase
    {
        [Name("RwbySleeper"), Group("RwbySleeper")]
        public class RwbySleeperCommands : ModuleBase
        {
            private readonly MongoClient _mongo;

            public RwbySleeperCommands(IServiceProvider provider)
            {
                _mongo = provider.GetService<MongoClient>();
            }
            
            [Command("Enable"), Alias("On, True, 1, Yes")]
            [MinPermission(AccessLevel.ServerModerator)]
            public async Task On()
            {
                var collection = _mongo.GetCollection<Settings>(Context.Client);
                var settings = await collection.GetByGuildAsync(Context.Guild.Id);

                if (!settings.RwbySleeper)
                {
                    settings.RwbySleeper = true;
                    await collection.SaveAsync(settings);
                    await Context.ReplyAsync("Rwby Sleeper has been Enabled!");
                }
                else await Context.ReplyAsync("Rwby Sleeper is already Enabled");
            }

            [Command("Disable"), Alias("Off, False, 0, No")]
            [MinPermission(AccessLevel.ServerModerator)]
            public async Task Off()
            {
                var collection = _mongo.GetCollection<Settings>(Context.Client);
                var settings = await collection.GetByGuildAsync(Context.Guild.Id);

                if (settings.RwbySleeper)
                {
                    settings.RwbySleeper = false;
                    await collection.SaveAsync(settings);
                    await Context.ReplyAsync("Rwby Sleeper has been Disabled!");
                }
                else await Context.ReplyAsync("Rwby Sleeper is already Disabled");
            }
        }
    }
}