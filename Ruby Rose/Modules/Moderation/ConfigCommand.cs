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
        public async Task Config(bool CustomReactions, bool ResultAnnounce)
        {
            var sb = new StringBuilder();
            SettingsManager.CustomReactions.AddOrUpdate(Context.Guild.Id, CustomReactions, (key, oldvalue) => CustomReactions);
            SettingsManager.ResultAnnounce.AddOrUpdate(Context.Guild.Id, ResultAnnounce, (key, oldvalue) => ResultAnnounce);

            sb.AppendLine($"Settings for {Context.Guild.Name} are now:");
            sb.AppendLine("```");
            sb.AppendLine($"CustomReactions = {CustomReactions}");
            sb.AppendLine($"Announce_ExecutionResult = {ResultAnnounce}");
            sb.Append("```");

            await SettingsManager.SaveSettings();

            await ReplyAsync($"{sb}");
        }
    }
}