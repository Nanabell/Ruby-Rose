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
    public class RwbyFight : ModuleBase
    {
        [Name("RwbyFight"), Group("RwbyFight")]
        public class RwbyFightSettingsCommands : ModuleBase
        {
            private readonly MongoClient _mongo;

            public RwbyFightSettingsCommands(IServiceProvider provider)
            {
                _mongo = provider.GetService<MongoClient>();
            }

            [Command("Enable"), Alias("On, True, 1, Yes")]
            [MinPermission(AccessLevel.ServerModerator)]
            public async Task On()
            {
                var allsettings = _mongo.GetCollection<Settings>(Context.Client);
                var settings = await allsettings.GetByGuildAsync(Context.Guild.Id);

                if (!settings.RwbyFight)
                {
                    settings.RwbyFight = true;
                    await allsettings.SaveAsync(settings);
                    await Context.ReplyAsync("Rwby Fight has been Enabled!");
                }
                else await Context.ReplyAsync("Rwby Fight is already Enabled");
            }

            [Command("Disable"), Alias("Off, False, 0, No")]
            [MinPermission(AccessLevel.ServerModerator)]
            public async Task Off()
            {
                var allsettings = _mongo.GetCollection<Settings>(Context.Client);
                var settings = await allsettings.GetByGuildAsync(Context.Guild.Id);

                if (settings.RwbyFight)
                {
                    settings.RwbyFight = false;
                    await allsettings.SaveAsync(settings);
                    await Context.ReplyAsync("Rwby Fight has been Disabled!");
                }
                else await Context.ReplyAsync("Rwby Fight is already Disabled");
            }
        }
    }
}