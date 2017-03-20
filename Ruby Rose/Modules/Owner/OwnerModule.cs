using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace RubyRose.Modules.Owner
{
    [Name("Owner"), Group]
    public class OwnerModule : ModuleBase
    {
        [Command("BotInfo")]
        [RequireOwner]
        public async Task Info()
        {
            var application = await Context.Client.GetApplicationInfoAsync();
            var discordSocketClient = Context.Client as DiscordSocketClient;
            if (discordSocketClient != null)
                await ReplyAsync(
                    $"{Format.Bold("Info")}\n" +
                    $"- Author: {application.Owner.Username} (ID {application.Owner.Id})\n" +
                    $"- Library: Discord.Net ({DiscordConfig.Version})\n" +
                    $"- Uptime: {GetUptime()}\n" +
                    "- Version: 1.6-BETA\n\n" +

                    $"{Format.Bold("Stats")}\n" +
                    $"- Heap Size: {GetHeapSize()} MB\n" +
                    $"- Guilds: {discordSocketClient.Guilds.Count}\n" +
                    $"- Channels: {discordSocketClient.Guilds.Sum(g => g.Channels.Count)}\n" +
                    $"- Users: {discordSocketClient.Guilds.Sum(g => g.Users.Count)}"
                );
        }

        [Command("Goodbye")]
        [RequireOwner]
        public async Task Goodbye(IUser user)
        {
            await Context.Channel.SendFileAsync(@"Data/Images/byeshit.jpg", user.Mention);
        }

        [Command("Test")]
        [RequireOwner]
        public async Task Test([Remainder] string input = null)
        {
            await ReplyAsync("Updated Test");
        }

        [Command("Raw")]
        [RequireOwner]
        public async Task Raw([Remainder] string message)
        {
            await ReplyAsync($"```{message}```");
        }

        private static string GetUptime()
            => (DateTime.Now - Process.GetCurrentProcess().StartTime).ToString(@"dd\.hh\:mm\:ss");
        private static string GetHeapSize() => Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2).ToString(CultureInfo.InvariantCulture);
    }
}
