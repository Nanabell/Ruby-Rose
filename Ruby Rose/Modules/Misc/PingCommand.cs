using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using RubyRose.Common.Preconditions;

namespace RubyRose.Modules.Misc
{
    [Name("Misc"), Group]
    public class PingCommand : ModuleBase
    {
        [Command("Ping"), Alias("Pong")]
        [Summary("Latency Check")]
        [MinPermission(AccessLevel.User), RequireAllowed, Ratelimit(1, 5, Measure.Seconds)]
        public async Task Ping()
        {
            await ReplyAsync($"Pong! `[{((DiscordSocketClient) Context.Client).Latency}ms]`");
        }
    }
}