using Discord.Commands;
using Discord.WebSocket;
using MongoDB.Driver;
using RubyRose.Database;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RubyRose.Modules.Moderation
{
    [Name("Config")]
    public class RwbyFightSettings : ModuleBase
    {
        [Name("Rwby Fight"), Group("Rwby Fight")]
        public class RwbyFightSettingsCommands : ModuleBase
        {
            private readonly MongoClient _mongo;

            public RwbyFightSettingsCommands(IDependencyMap map)
            {
                _mongo = map.Get<MongoClient>();
            }

            [Command("Enable"), Alias("On, True, 1, Yes")]
            [MinPermission(AccessLevel.ServerModerator)]
            public async Task On()
            {
                var allsettings = _mongo.GetCollection<Settings>(Context.Client);
                var settings = await allsettings.GetByGuildAsync(Context.Guild);

                if (!settings.RwbyFight)
                {
                    settings.RwbyFight = true;
                    await allsettings.SaveAsync(settings);
                    await CommandHandler.ReloadResultAnnounce(Context.Client as DiscordSocketClient, _mongo);
                    await Context.ReplyAsync("Rwby Fight has been Enabled!");
                }
                else await Context.ReplyAsync("Rwby Fight is already Enabled");
            }

            [Command("Disable"), Alias("Off, False, 0, No")]
            [MinPermission(AccessLevel.ServerModerator)]
            public async Task Off()
            {
                var allsettings = _mongo.GetCollection<Settings>(Context.Client);
                var settings = await allsettings.GetByGuildAsync(Context.Guild);

                if (settings.RwbyFight)
                {
                    settings.RwbyFight = false;
                    await allsettings.SaveAsync(settings);
                    await CommandHandler.ReloadResultAnnounce(Context.Client as DiscordSocketClient, _mongo);
                    await Context.ReplyAsync("Rwby Fight has been Disabled!");
                }
                else await Context.ReplyAsync("Rwby Fight is already Disabled");
            }
        }
    }
}