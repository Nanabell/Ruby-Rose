using Discord.Commands;
using Discord.WebSocket;
using MongoDB.Driver;
using RubyRose.Common;
using RubyRose.Common.Preconditions;
using RubyRose.Custom_Reactions;
using RubyRose.Database;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RubyRose.Modules.Moderation
{
    [Name("Moderation"), Group("Config")]
    public class ConfigCommand : ModuleBase
    {
        private readonly MongoClient _mongo;

        public ConfigCommand(IDependencyMap map)
        {
            _mongo = map.Get<MongoClient>();
        }

        [Command]
        [MinPermission(AccessLevel.ServerModerator)]
        public async Task Config()
        {
            var settings = await _mongo.GetCollection<Settings>(Context.Client).GetListAsync(Context.Guild);
            var setting = settings.First();
            var sb = new StringBuilder();

            sb.AppendLine($"Settings for {Context.Guild.Name}:");
            sb.AppendLine("```");
            sb.AppendLine($"CustomReactions = {setting.CustomReactions}");
            sb.AppendLine($"Announce_ExecutionResult = {setting.ExecutionErrorAnnounce}");
            sb.Append("```");
            await ReplyAsync($"{sb}");
        }

        [Command("Set")]
        [MinPermission(AccessLevel.ServerModerator)]
        public async Task Config(bool CustomReactions, bool ExecutResultAnnouncements)
        {
            var settings = _mongo.GetCollection<Settings>(Context.Client);
            var setting = settings.Find(g => g.GuildId == Context.Guild.Id).First();
            var sb = new StringBuilder();

            setting.CustomReactions = CustomReactions;
            setting.ExecutionErrorAnnounce = ExecutResultAnnouncements;

            sb.AppendLine($"Settings for {Context.Guild.Name} are now:");
            sb.AppendLine("```");
            sb.AppendLine($"CustomReactions = {setting.CustomReactions}");
            sb.AppendLine($"Announce_ExecutionResult = {setting.ExecutionErrorAnnounce}");
            sb.Append("```");

            await settings.SaveAsync(setting);

            CommandHandler.MongoLoader(_mongo, Context.Client as DiscordSocketClient);
            RwbyFight.MongoLoader(_mongo, Context.Client as DiscordSocketClient);

            await ReplyAsync($"{sb}");
        }
    }
}