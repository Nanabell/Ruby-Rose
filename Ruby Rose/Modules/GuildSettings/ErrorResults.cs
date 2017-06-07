using System;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using RubyRose.Common;
using RubyRose.Common.Preconditions;
using RubyRose.Database;

namespace RubyRose.Modules.GuildSettings
{
    [Name("Config")]
    public class ErrorResults : ModuleBase
    {
        [Name("Errors"), Group("Errors")]
        public class ResultAnnounceSettingsCommands : ModuleBase
        {
            private readonly MongoClient _mongo;

            public ResultAnnounceSettingsCommands(IServiceProvider provider)
            {
                _mongo = provider.GetService<MongoClient>();
            }

            [Command("Enable"), Alias("On, True, 1, Yes")]
            [MinPermission(AccessLevel.ServerModerator)]
            public async Task On()
            {
                var allsettings = _mongo.GetCollection<Database.Models.Settings>(Context.Client);
                var settings = await allsettings.GetByGuildAsync(Context.Guild.Id);

                if (!settings.IsErrorReporting)
                {
                    settings.IsErrorReporting = true;
                    await allsettings.SaveAsync(settings);
                    await Context.ReplyAsync("Error Message have been Enabled!");
                }
                else await Context.ReplyAsync("Error Message are already Enabled");
            }

            [Command("Disable"), Alias("Off, False, 0, No")]
            [MinPermission(AccessLevel.ServerModerator)]
            public async Task Off()
            {
                var allsettings = _mongo.GetCollection<Database.Models.Settings>(Context.Client);
                var settings = await allsettings.GetByGuildAsync(Context.Guild.Id);

                if (settings.IsErrorReporting)
                {
                    settings.IsErrorReporting = false;
                    await allsettings.SaveAsync(settings);
                    await Context.ReplyAsync("Error Messages have been Disabled!");
                }
                else await Context.ReplyAsync("Error Messages are already Disabled");
            }
        }
    }
}