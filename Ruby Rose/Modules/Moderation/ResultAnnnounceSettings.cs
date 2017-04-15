using Discord.Commands;
using Discord.WebSocket;
using MongoDB.Driver;
using RubyRose.Common.Preconditions;
using RubyRose.Database;
using System.Threading.Tasks;
using RubyRose.Common;
using RubyRose.Database.Models;

namespace RubyRose.Modules.Moderation
{
    [Name("Config")]
    public class ResultAnnnounceSettings : ModuleBase
    {
        [Name("ResultAnnounce"), Group("ResultAnnounce")]
        public class ResultAnnounceSettingsCommands : ModuleBase
        {
            private readonly MongoClient _mongo;

            public ResultAnnounceSettingsCommands(IDependencyMap map)
            {
                _mongo = map.Get<MongoClient>();
            }

            [Command("Enable"), Alias("On, True, 1, Yes")]
            [MinPermission(AccessLevel.ServerModerator)]
            public async Task On()
            {
                var allsettings = _mongo.GetCollection<Settings>(Context.Client);
                var settings = await allsettings.GetByGuildAsync(Context.Guild);

                if (!settings.ResultAnnounce)
                {
                    settings.ResultAnnounce = true;
                    await allsettings.SaveAsync(settings);
                    await CommandHandler.ReloadResultAnnounce(Context.Client as DiscordSocketClient, _mongo);
                    await Context.ReplyAsync("Result Announce has been Enabled!");
                }
                else await Context.ReplyAsync("Result Announce is already Enabled");
            }

            [Command("Disable"), Alias("Off, False, 0, No")]
            [MinPermission(AccessLevel.ServerModerator)]
            public async Task Off()
            {
                var allsettings = _mongo.GetCollection<Settings>(Context.Client);
                var settings = await allsettings.GetByGuildAsync(Context.Guild);

                if (settings.ResultAnnounce)
                {
                    settings.ResultAnnounce = false;
                    await allsettings.SaveAsync(settings);
                    await CommandHandler.ReloadResultAnnounce(Context.Client as DiscordSocketClient, _mongo);
                    await Context.ReplyAsync("Result Announce has been Disabled!");
                }
                else await Context.ReplyAsync("Result Announce is already Disabled");
            }
        }
    }
}