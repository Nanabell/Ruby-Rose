using Discord.Commands;
using MongoDB.Driver;
using System.Text;
using System.Threading.Tasks;
using RubyRose.Database;
using RubyRose.Services;
using Discord.WebSocket;
using RubyRose.Common;
using RubyRose.Common.Preconditions;
using RubyRose.Database.Models;
using RubyRose.Services.BadWordsFilter;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace RubyRose.Modules.Moderation.BadWords
{
    [Name("Moderation")]
    public class BadWordsFilter : ModuleBase
    {
        [Name("BadWords"), Group("BadWords")]
        public class BadWordsCommands : ModuleBase
        {
            private readonly MongoClient _mongo;

            public BadWordsCommands(IServiceProvider provider)
            {
                _mongo = provider.GetService<MongoClient>();
            }

            [Command]
            [MinPermission(AccessLevel.User)]
            public async Task Filters()
            {
                var settings = await _mongo.GetCollection<Settings>(Context.Client).GetByGuildAsync(Context.Guild);
                var sb = new StringBuilder();

                sb.AppendLine("List of Bad Words");
                sb.AppendLine("```");
                foreach (var word in settings.BadWords)
                {
                    sb.AppendLine(word);
                }
                sb.AppendLine("```");

                await Context.ReplyAsync($"\n {sb}");
            }

            [Command("Add")]
            [MinPermission(AccessLevel.ServerModerator)]
            public async Task Add([Remainder]string filter)
            {
                var allSettings = _mongo.GetCollection<Settings>(Context.Client);
                var settings = await allSettings.GetByGuildAsync(Context.Guild);

                if (!settings.BadWords.Contains(filter))
                {
                    settings.BadWords.Add(filter);
                    await allSettings.SaveAsync(settings);
                    await Context.ReplyAsync($"Bad Word `{filter}` added to the Database");
                    await BadWordsFilterService.ReloadBadWords(Context.Client as DiscordSocketClient, _mongo);
                }
                else
                {
                    await Context.ReplyAsync($"Bad Word `{filter}` already in Database!");
                }
            }

            [Command("Remove")]
            [MinPermission(AccessLevel.ServerModerator)]
            public async Task Remove([Remainder]string filter)
            {
                var allSettings = _mongo.GetCollection<Settings>(Context.Client);
                var settings = await allSettings.GetByGuildAsync(Context.Guild);

                if (settings.BadWords.Contains(filter))
                {
                    settings.BadWords.Remove(filter);
                    await allSettings.SaveAsync(settings);
                    await Context.ReplyAsync($"Bad Word `{filter}` dropped from Database");
                    await BadWordsFilterService.ReloadBadWords(Context.Client as DiscordSocketClient, _mongo);
                }
                else
                {
                    await Context.ReplyAsync($"Bad Word `{filter}` not in Database");
                }
            }
        }
    }
}